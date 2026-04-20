using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whycespace.Engines.T1M.Core.Lifecycle;
using Whycespace.Engines.T1M.Core.StepExecutor;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Admission;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Core.WorkflowEngine;

/// <summary>
/// T1M workflow orchestrator. Executes a workflow definition step-by-step and
/// emits domain lifecycle events through <see cref="WorkflowLifecycleEventFactory"/>
/// into the <see cref="IDomainEventSink"/> on the execution context.
///
/// The engine NEVER mutates aggregate state directly (engine.guard rule 3).
/// Lifecycle transitions are produced as event records by the factory; the
/// runtime drains them through persist → chain → outbox.
///
/// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): the engine now bounds
/// every execution with a declared two-tier timeout discipline:
///
///   - An execution-level CTS linked to the upstream request/host
///     token, bounded by <see cref="WorkflowOptions.MaxExecutionMs"/>.
///     Caps the cumulative wall time of the entire workflow even if
///     every individual step finishes under its own ceiling.
///   - A per-step CTS linked to the execution CTS, bounded by
///     <see cref="WorkflowOptions.PerStepTimeoutMs"/>. Caps each
///     <see cref="IWorkflowStep.ExecuteAsync"/> call.
///
/// On declared expiry the engine throws the typed
/// <see cref="WorkflowTimeoutException"/> — the canonical RETRYABLE
/// REFUSAL counterpart to <see cref="WorkflowSaturatedException"/>.
/// Caller-driven cancellation (request abort, host shutdown) propagates
/// as <see cref="OperationCanceledException"/> without wrapping so host
/// shutdown semantics are preserved.
/// </summary>
public sealed class T1MWorkflowEngine : IWorkflowEngine
{
    // R3.A.1 / R-WORKFLOW-OBSERVABILITY-01: reuse the existing
    // Whycespace.Workflow meter (declared by WorkflowAdmissionGate) so
    // engine instrumentation composes with admission-gate signals in
    // a single operator dashboard. Multiple Meter instances sharing the
    // same name/version are collapsed by listeners (OTel, Prometheus
    // exporter, dotnet-counters).
    private static readonly Meter Meter = new("Whycespace.Workflow", "1.0");

    private static readonly Histogram<double> ExecutionDuration =
        Meter.CreateHistogram<double>("workflow.execution.duration", unit: "ms");

    private static readonly Histogram<double> StepDuration =
        Meter.CreateHistogram<double>("workflow.step.duration", unit: "ms");

    private static readonly Counter<long> ExecutionCompleted =
        Meter.CreateCounter<long>("workflow.execution.completed");

    // R3.A.2 / R-WORKFLOW-STEP-RETRY-OBSERVABILITY-01: retry-attempts
    // counter. Incremented exactly once per retry (i.e., on each
    // attempt after the first). A step that succeeds on its first try
    // contributes zero; a step that succeeds on attempt 3 contributes 2.
    private static readonly Counter<long> StepRetryAttempts =
        Meter.CreateCounter<long>("workflow.step.retry_attempts");

    // R3.A.5 / R-WORKFLOW-STEP-EXCEPTION-COUNTER-01: terminal-failures
    // counter. Incremented once per step that fast-fails with a
    // terminal-classified exception (see WorkflowStepFailureClassifier).
    // Tagged {workflow_name, step_name, category}. Operators compute
    // terminal_rate = terminal_failures / workflow.step.duration
    // failed_outcome_count for the failure breakdown.
    private static readonly Counter<long> StepTerminalFailures =
        Meter.CreateCounter<long>("workflow.step.terminal_failures");

    // R-WORKFLOW-OBSERVABILITY-01: canonical outcome vocabulary. Exactly
    // one of these tags every emission. Low cardinality by construction.
    private const string OutcomeSuccess = "success";
    private const string OutcomeFailed = "failed";
    private const string OutcomeTimeoutStep = "timeout_step";
    private const string OutcomeTimeoutExecution = "timeout_execution";
    private const string OutcomeCancelled = "cancelled";

    private readonly WorkflowStepExecutor _stepExecutor;
    private readonly WorkflowLifecycleEventFactory _lifecycleFactory;
    private readonly WorkflowOptions _options;

    public T1MWorkflowEngine(
        WorkflowStepExecutor stepExecutor,
        WorkflowLifecycleEventFactory lifecycleFactory,
        WorkflowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.PerStepTimeoutMs < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.PerStepTimeoutMs,
                "WorkflowOptions.PerStepTimeoutMs must be at least 1.");
        if (options.MaxExecutionMs < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.MaxExecutionMs,
                "WorkflowOptions.MaxExecutionMs must be at least 1.");
        // R3.A.2 / R-WORKFLOW-STEP-RETRY-01: validate new retry tunables.
        if (options.StepRetryMaxAttempts < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.StepRetryMaxAttempts,
                "WorkflowOptions.StepRetryMaxAttempts must be at least 1.");
        if (options.StepRetryBaseBackoffMs < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.StepRetryBaseBackoffMs,
                "WorkflowOptions.StepRetryBaseBackoffMs must be at least 1.");
        if (options.StepRetryMaxBackoffMs < options.StepRetryBaseBackoffMs)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.StepRetryMaxBackoffMs,
                "WorkflowOptions.StepRetryMaxBackoffMs must be >= StepRetryBaseBackoffMs.");

        _stepExecutor = stepExecutor;
        _lifecycleFactory = lifecycleFactory;
        _options = options;
    }

    public async Task<WorkflowExecutionResult> ExecuteAsync(
        WorkflowDefinition definition,
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var startIndex = context.CurrentStepIndex;

        if (startIndex == 0)
        {
            context.EmitEvent(_lifecycleFactory.Started(context.WorkflowId, context.WorkflowName, context.Payload));
        }

        // phase1.5-S5.2.3 / TC-7: execution-level linked CTS bounded
        // by MaxExecutionMs. Linked to the upstream request/host token
        // so caller-driven cancellation still wins. Disposed at the
        // end of ExecuteAsync regardless of outcome.
        using var executionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        executionCts.CancelAfter(_options.MaxExecutionMs);

        // R3.A.1 / R-WORKFLOW-OBSERVABILITY-01: start the execution-level
        // duration marker. Stopwatch is monotonic and deliberately
        // exempted from the IClock determinism rule per
        // R-WORKFLOW-OBSERVABILITY-DETERMINISM-NOTE-01 — this is
        // observability-only and never flows into audit data.
        var executionStartTicks = Stopwatch.GetTimestamp();
        var workflowNameTag = new KeyValuePair<string, object?>("workflow_name", context.WorkflowName);

        for (var i = startIndex; i < definition.Steps.Count; i++)
        {
            var stepDefinition = definition.Steps[i];
            context.CurrentStepIndex = i;

            // R3.A.2 / R-WORKFLOW-STEP-RETRY-01: bounded retry loop
            // around step execution. Each attempt gets a fresh per-step
            // linked CTS; timeouts/cancellations break out of the loop
            // without retrying. Soft failures (stepResult.IsSuccess ==
            // false) and hard failures (non-cancel, non-timeout
            // exception) retry up to StepRetryMaxAttempts-1 more times
            // with exponential backoff, sleeping under executionCts.
            WorkflowStepResult stepResult;
            string? lastSoftFailureError = null;
            Exception? lastHardFailureException = null;

            var attemptIndex = 0;
            while (true)
            {
                attemptIndex++;

                // phase1.5-S5.2.3 / TC-7: per-step linked CTS bounded by
                // PerStepTimeoutMs. Linked to executionCts so the per-step
                // ceiling never extends past the overall execution ceiling
                // and so caller cancellation propagates through both tiers.
                //
                // R3.A.2: refreshed per attempt — each retry gets the
                // full per-step window, not the remainder of the previous
                // attempt. Execution-level CTS still caps cumulative work.
                using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(executionCts.Token);
                stepCts.CancelAfter(_options.PerStepTimeoutMs);

                // R3.A.1 / R-WORKFLOW-OBSERVABILITY-01: per-ATTEMPT
                // duration marker. Each attempt emits its own
                // workflow.step.duration sample with the attempt's
                // outcome so histogram sample count = total attempts.
                var stepStartTicks = Stopwatch.GetTimestamp();

                try
                {
                    stepResult = await _stepExecutor.ExecuteAsync(stepDefinition, context, stepCts.Token);
                }
                catch (OperationCanceledException cancelEx) when (
                    cancellationToken.IsCancellationRequested)
                {
                    // Caller-driven cancellation: propagate as-is, no wrap.
                    // R-WORKFLOW-STEP-RETRY-NON-RETRYABLE-EXCLUSION-01: do NOT retry.
                    RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeCancelled);
                    RecordExecution(executionStartTicks, workflowNameTag, OutcomeCancelled);
                    // R3.A.4 / R-WORKFLOW-CANCELLATION-ENGINE-EMISSION-01:
                    // emit the lifecycle event BEFORE re-throwing so the
                    // event stream distinguishes deliberate cancellation
                    // from crash/incomplete flow. The re-throw contract
                    // is preserved — callers still receive the OCE
                    // unchanged; cancellation still propagates up.
                    var cancelReason = $"caller_cancellation: {cancelEx.GetType().Name}: {cancelEx.Message}";
                    context.EmitEvent(_lifecycleFactory.Cancelled(
                        context.WorkflowId, stepDefinition.StepName, cancelReason));
                    throw;
                }
                catch (OperationCanceledException) when (
                    executionCts.IsCancellationRequested && !stepCts.IsCancellationRequested)
                {
                    // Execution-level deadline expired.
                    // R-WORKFLOW-STEP-RETRY-NON-RETRYABLE-EXCLUSION-01: do NOT retry.
                    RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeTimeoutExecution);
                    RecordExecution(executionStartTicks, workflowNameTag, OutcomeTimeoutExecution);
                    throw new WorkflowTimeoutException(
                        kind: "execution",
                        stepName: null,
                        timeoutMs: _options.MaxExecutionMs,
                        retryAfterSeconds: _options.RetryAfterSeconds,
                        message: $"Workflow '{context.WorkflowName}' exceeded MaxExecutionMs={_options.MaxExecutionMs}. No bypass allowed.");
                }
                catch (OperationCanceledException) when (
                    stepCts.IsCancellationRequested)
                {
                    // Per-step deadline expired.
                    // R-WORKFLOW-STEP-RETRY-NON-RETRYABLE-EXCLUSION-01: do NOT retry.
                    RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeTimeoutStep);
                    RecordExecution(executionStartTicks, workflowNameTag, OutcomeTimeoutStep);
                    throw new WorkflowTimeoutException(
                        kind: "step",
                        stepName: stepDefinition.StepName,
                        timeoutMs: _options.PerStepTimeoutMs,
                        retryAfterSeconds: _options.RetryAfterSeconds,
                        message: $"Workflow step '{stepDefinition.StepName}' exceeded PerStepTimeoutMs={_options.PerStepTimeoutMs}. No bypass allowed.");
                }
                catch (Exception hardEx)
                {
                    // Hard failure (non-cancel, non-timeout).
                    // R3.A.5 / R-WORKFLOW-STEP-EXCEPTION-CLASSIFICATION-01:
                    // classify before retrying. Terminal failures fast-fail
                    // without burning attempt budget; retryable failures
                    // preserve the R3.A.2 loop.
                    RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeFailed);
                    lastHardFailureException = hardEx;

                    var classification = WorkflowStepFailureClassifier.Classify(hardEx);
                    if (classification == WorkflowStepFailureClassification.Terminal)
                    {
                        // R-WORKFLOW-STEP-EXCEPTION-COUNTER-01: record
                        // terminal failure with the category tag so
                        // operator dashboards can break down the failure
                        // distribution.
                        var categoryTag = WorkflowStepFailureClassifier.CategoryTag(hardEx);
                        StepTerminalFailures.Add(1,
                            new KeyValuePair<string, object?>("workflow_name", context.WorkflowName),
                            new KeyValuePair<string, object?>("step_name", stepDefinition.StepName),
                            new KeyValuePair<string, object?>("category", categoryTag));

                        RecordExecution(executionStartTicks, workflowNameTag, OutcomeFailed);
                        var terminalReason = $"Step '{stepDefinition.StepName}' failed terminally with category={categoryTag}: {hardEx.GetType().Name}: {hardEx.Message}";
                        context.EmitEvent(_lifecycleFactory.Failed(context.WorkflowId, stepDefinition.StepName, terminalReason));
                        return WorkflowExecutionResult.Failure(stepDefinition.StepName, terminalReason);
                    }

                    // Retryable hard failure — existing R3.A.2 loop.
                    if (attemptIndex < _options.StepRetryMaxAttempts)
                    {
                        StepRetryAttempts.Add(1,
                            new KeyValuePair<string, object?>("workflow_name", context.WorkflowName),
                            new KeyValuePair<string, object?>("step_name", stepDefinition.StepName));
                        await SleepBackoffAsync(attemptIndex, executionCts.Token);
                        continue;
                    }
                    // Exhausted. Emit final failed outcome + lifecycle event.
                    RecordExecution(executionStartTicks, workflowNameTag, OutcomeFailed);
                    var exhaustReason = $"Step '{stepDefinition.StepName}' exhausted {_options.StepRetryMaxAttempts} attempt(s): {hardEx.GetType().Name}: {hardEx.Message}";
                    context.EmitEvent(_lifecycleFactory.Failed(context.WorkflowId, stepDefinition.StepName, exhaustReason));
                    return WorkflowExecutionResult.Failure(stepDefinition.StepName, exhaustReason);
                }

                // Soft failure path.
                if (!stepResult.IsSuccess)
                {
                    RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeFailed);
                    lastSoftFailureError = stepResult.Error ?? $"Step '{stepDefinition.StepName}' failed.";
                    if (attemptIndex < _options.StepRetryMaxAttempts)
                    {
                        StepRetryAttempts.Add(1,
                            new KeyValuePair<string, object?>("workflow_name", context.WorkflowName),
                            new KeyValuePair<string, object?>("step_name", stepDefinition.StepName));
                        await SleepBackoffAsync(attemptIndex, executionCts.Token);
                        continue;
                    }
                    // Exhausted. Emit final failed outcome + lifecycle event.
                    RecordExecution(executionStartTicks, workflowNameTag, OutcomeFailed);
                    var exhaustReason = _options.StepRetryMaxAttempts > 1
                        ? $"Step '{stepDefinition.StepName}' exhausted {_options.StepRetryMaxAttempts} attempt(s): {lastSoftFailureError}"
                        : lastSoftFailureError;
                    context.EmitEvent(_lifecycleFactory.Failed(context.WorkflowId, stepDefinition.StepName, exhaustReason));
                    return WorkflowExecutionResult.Failure(stepDefinition.StepName, exhaustReason);
                }

                // Successful step on this attempt — record duration + break out of retry loop.
                RecordStepDuration(stepStartTicks, context.WorkflowName, stepDefinition.StepName, OutcomeSuccess);
                break;
            }

            context.StepOutputs[stepDefinition.StepName] = stepResult.Output;

            if (stepResult.Events.Count > 0)
            {
                context.AccumulatedEvents.AddRange(stepResult.Events);
            }

            context.ExecutionHash = ComputeStepHash(
                context.ExecutionHash,
                stepDefinition.StepId,
                stepResult.Output);

            context.EmitEvent(_lifecycleFactory.StepCompleted(
                context.WorkflowId, i, stepDefinition.StepName, context.ExecutionHash, stepResult.Output));
        }

        RecordExecution(executionStartTicks, workflowNameTag, OutcomeSuccess);
        context.EmitEvent(_lifecycleFactory.Completed(context.WorkflowId, context.ExecutionHash));

        return WorkflowExecutionResult.Success(
            context.WorkflowOutput,
            context.AccumulatedEvents.AsReadOnly());
    }

    /// <summary>
    /// R3.A.1 / R-WORKFLOW-OBSERVABILITY-01: canonical per-step
    /// duration recorder. Called from every step exit path (success,
    /// failed, timeout_step, timeout_execution, cancelled) so the
    /// histogram sample count equals the step-completion count.
    /// </summary>
    private static void RecordStepDuration(
        long startTicks, string workflowName, string stepName, string outcome)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds;
        StepDuration.Record(elapsedMs,
            new KeyValuePair<string, object?>("workflow_name", workflowName),
            new KeyValuePair<string, object?>("step_name", stepName),
            new KeyValuePair<string, object?>("outcome", outcome));
    }

    /// <summary>
    /// R3.A.1 / R-WORKFLOW-OBSERVABILITY-01 + R-WORKFLOW-OBSERVABILITY-COMPLETION-COUNTER-01:
    /// canonical execution-exit recorder. Emits both the duration
    /// histogram and the completion counter in lockstep so operators
    /// can compute per-outcome rates without deriving from the
    /// histogram sample count alone.
    /// </summary>
    private static void RecordExecution(
        long startTicks, KeyValuePair<string, object?> workflowNameTag, string outcome)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(startTicks).TotalMilliseconds;
        var outcomeTag = new KeyValuePair<string, object?>("outcome", outcome);
        ExecutionDuration.Record(elapsedMs, workflowNameTag, outcomeTag);
        ExecutionCompleted.Add(1, workflowNameTag, outcomeTag);
    }

    /// <summary>
    /// R3.A.2 / R-WORKFLOW-STEP-RETRY-01: exponential backoff delay
    /// between retry attempts. <paramref name="failedAttemptIndex"/>
    /// is the 1-based number of the attempt that just failed (so the
    /// first retry waits <c>BaseBackoff × 2^0 = BaseBackoff</c>, the
    /// second retry <c>BaseBackoff × 2^1</c>, and so on, capped at
    /// <c>MaxBackoff</c>). Delay sleeps under the execution-level CTS
    /// token so caller cancellation + MaxExecutionMs interrupt waiting
    /// retries. An <see cref="OperationCanceledException"/> propagates
    /// unchanged — the caller loop's next iteration will catch it via
    /// the existing timeout/cancel filters.
    /// </summary>
    private async Task SleepBackoffAsync(int failedAttemptIndex, CancellationToken ct)
    {
        var exponent = Math.Pow(2.0, Math.Max(0, failedAttemptIndex - 1));
        var rawMs = _options.StepRetryBaseBackoffMs * exponent;
        var cappedMs = Math.Min(rawMs, _options.StepRetryMaxBackoffMs);
        await Task.Delay(TimeSpan.FromMilliseconds(cappedMs), ct);
    }

    private static string ComputeStepHash(string previousHash, string stepId, object? output)
    {
        var outputHash = output is not null
            ? JsonSerializer.Serialize(output)
            : string.Empty;

        var input = $"{previousHash}{stepId}{outputHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
