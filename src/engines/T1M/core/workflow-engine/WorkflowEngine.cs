using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Whyce.Engines.T1M.Core.Lifecycle;
using Whyce.Engines.T1M.Core.StepExecutor;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Admission;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Core.WorkflowEngine;

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

        for (var i = startIndex; i < definition.Steps.Count; i++)
        {
            var stepDefinition = definition.Steps[i];
            context.CurrentStepIndex = i;

            // phase1.5-S5.2.3 / TC-7: per-step linked CTS bounded by
            // PerStepTimeoutMs. Linked to executionCts so the per-step
            // ceiling never extends past the overall execution ceiling
            // and so caller cancellation propagates through both tiers.
            using var stepCts = CancellationTokenSource.CreateLinkedTokenSource(executionCts.Token);
            stepCts.CancelAfter(_options.PerStepTimeoutMs);

            WorkflowStepResult stepResult;
            try
            {
                stepResult = await _stepExecutor.ExecuteAsync(stepDefinition, context, stepCts.Token);
            }
            catch (OperationCanceledException) when (
                cancellationToken.IsCancellationRequested)
            {
                // Caller-driven cancellation: propagate as-is, no wrap.
                throw;
            }
            catch (OperationCanceledException) when (
                executionCts.IsCancellationRequested && !stepCts.IsCancellationRequested)
            {
                // Execution-level deadline expired (the step itself
                // hadn't yet hit its per-step ceiling). Canonical
                // RETRYABLE REFUSAL.
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
                // Per-step deadline expired. Canonical RETRYABLE REFUSAL.
                throw new WorkflowTimeoutException(
                    kind: "step",
                    stepName: stepDefinition.StepName,
                    timeoutMs: _options.PerStepTimeoutMs,
                    retryAfterSeconds: _options.RetryAfterSeconds,
                    message: $"Workflow step '{stepDefinition.StepName}' exceeded PerStepTimeoutMs={_options.PerStepTimeoutMs}. No bypass allowed.");
            }

            if (!stepResult.IsSuccess)
            {
                var error = stepResult.Error ?? $"Step '{stepDefinition.StepName}' failed.";
                context.EmitEvent(_lifecycleFactory.Failed(context.WorkflowId, stepDefinition.StepName, error));
                return WorkflowExecutionResult.Failure(stepDefinition.StepName, error);
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

        context.EmitEvent(_lifecycleFactory.Completed(context.WorkflowId, context.ExecutionHash));

        return WorkflowExecutionResult.Success(
            context.WorkflowOutput,
            context.AccumulatedEvents.AsReadOnly());
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
