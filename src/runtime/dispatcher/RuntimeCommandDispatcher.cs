using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Infrastructure.Persistence;
using Whyce.Shared.Contracts.Runtime;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whyce.Runtime.Dispatcher;

/// <summary>
/// Routes commands to appropriate handlers: workflow orchestration (T1M) or
/// direct engine execution (T2E). The dispatcher is the terminal step in the
/// middleware pipeline — reached only after all guards and policy have passed.
///
/// As of the workflow eventification refactor, the dispatcher no longer owns
/// any workflow state — lifecycle transitions are domain events emitted by the
/// T1M engine via WorkflowLifecycleEventFactory and persisted by the runtime
/// persist → chain → outbox pipeline.
///
/// WorkflowResumeCommand is handled via IWorkflowExecutionReplayService, which
/// reconstructs WorkflowExecutionAggregate from the event store inside the T1M
/// engine layer. The dispatcher itself remains domain-agnostic per runtime.guard
/// rule 11.R-DOM-01 — it depends only on the shared contract.
/// </summary>
public sealed class RuntimeCommandDispatcher : ICommandDispatcher
{
    private readonly IEngineRegistry _engineRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventStore _eventStore;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowExecutionReplayService _replayService;
    private readonly WorkflowAdmissionGate _workflowAdmissionGate;

    // phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): the dispatcher
    // now consults the declared workflow admission gate before
    // invoking the workflow engine. The gate enforces per-workflow-name
    // and per-tenant in-flight ceilings sized from WorkflowOptions
    // and throws WorkflowSaturatedException on overflow — the typed
    // RETRYABLE REFUSAL path mapped to HTTP 503 + Retry-After at the
    // API edge. Pre-KC-6 the dispatcher had no concurrency primitive
    // at the workflow seam.
    public RuntimeCommandDispatcher(
        IEngineRegistry engineRegistry,
        IServiceProvider serviceProvider,
        IEventStore eventStore,
        IWorkflowEngine workflowEngine,
        IWorkflowRegistry workflowRegistry,
        IWorkflowExecutionReplayService replayService,
        WorkflowAdmissionGate workflowAdmissionGate)
    {
        ArgumentNullException.ThrowIfNull(workflowAdmissionGate);
        _engineRegistry = engineRegistry;
        _serviceProvider = serviceProvider;
        _eventStore = eventStore;
        _workflowEngine = workflowEngine;
        _workflowRegistry = workflowRegistry;
        _replayService = replayService;
        _workflowAdmissionGate = workflowAdmissionGate;
    }

    public async Task<CommandResult> DispatchAsync(object command, CommandContext context, CancellationToken cancellationToken = default)
    {
        // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): the
        // dispatcher entry now accepts a CancellationToken so the
        // upstream pipeline can carry it through. Internal helpers
        // (ExecuteWorkflowAsync, ResumeWorkflowAsync, ExecuteEngineAsync)
        // continue to drop the token in this pass — engine-side and
        // workflow-step token threading is TC-5 / TC-7 / TC-8.
        if (command is WorkflowStartCommand workflowCommand)
        {
            return await ExecuteWorkflowAsync(workflowCommand, context, cancellationToken);
        }

        if (command is WorkflowResumeCommand resumeCommand)
        {
            return await ResumeWorkflowAsync(resumeCommand, context, cancellationToken);
        }

        return await ExecuteEngineAsync(command, context, cancellationToken);
    }

    private async Task<CommandResult> ExecuteWorkflowAsync(WorkflowStartCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        var definition = BuildDefinition(command.WorkflowName);
        if (definition is null)
        {
            return CommandResult.Failure($"No workflow registered for '{command.WorkflowName}'.");
        }

        // phase1.5-S5.2.2 / KC-6: acquire the workflow admission lease
        // BEFORE constructing the execution context. The using-block
        // releases both per-workflow-name and per-tenant permits when
        // the engine call returns (success or failure). On overflow
        // the gate throws WorkflowSaturatedException which bubbles
        // untouched to the API edge handler.
        // phase1.5-S5.2.3 / TC-8 (WORKFLOW-GATE-CT-01): forward the
        // real request/host-shutdown CancellationToken into the
        // admission gate so a saturated gate honors caller cancellation
        // instead of blocking on a default token.
        using var admissionLease = await _workflowAdmissionGate.AcquireAsync(
            command.WorkflowName, context.TenantId, cancellationToken);

        var executionContext = new WorkflowExecutionContext
        {
            WorkflowId = command.Id,
            CorrelationId = context.CorrelationId,
            WorkflowName = command.WorkflowName,
            Payload = command.Payload,
            IdentityId = context.IdentityId,
            PolicyDecision = context.PolicyDecisionHash
        };

        // phase1.5-S5.2.3 / TC-7: forward CT into the workflow engine so the
        // execution-level / per-step linked CTSs honor the upstream token.
        var result = await _workflowEngine.ExecuteAsync(definition, executionContext, cancellationToken);

        // Whether success or failure, lifecycle events have been accumulated on the context
        // (Started, StepCompleted*, and either Completed or Failed). Return them so the
        // runtime persist → chain → outbox pipeline records the entire trail.
        var events = executionContext.AccumulatedEvents.AsReadOnly();

        return result.IsSuccess
            ? CommandResult.Success(events, result.Output, eventsRequirePersistence: events.Count > 0)
            : CommandResult.Success(events, eventsRequirePersistence: events.Count > 0);
    }

    private async Task<CommandResult> ResumeWorkflowAsync(WorkflowResumeCommand command, CommandContext context, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(command.WorkflowId, out var workflowExecutionId))
        {
            return CommandResult.Failure(
                $"WorkflowResumeCommand.WorkflowId '{command.WorkflowId}' is not a valid Guid.");
        }

        var state = await _replayService.ReplayAsync(workflowExecutionId);
        if (state is null)
        {
            return CommandResult.Failure(
                $"No workflow execution events found for '{workflowExecutionId}'.");
        }

        // phase1.5-S5.2.2 / KC-6: resume is also gated. The lease is
        // keyed by the same workflow name as the original Start so a
        // saturated workflow refuses both new starts and resume
        // attempts uniformly.
        // phase1.5-S5.2.3 / TC-8: same token threading on the resume path.
        using var admissionLease = await _workflowAdmissionGate.AcquireAsync(
            state.WorkflowName, context.TenantId, cancellationToken);

        // H8 — resume is ONLY valid from the Failed state. The dispatcher does
        // not "continue running" workflows; that path was a previous semantic
        // bug. A Failed workflow must be transitioned through the aggregate's
        // Resume() invariant which raises WorkflowExecutionResumedEvent before
        // any new step events are emitted.
        if (!string.Equals(state.Status, "Failed", StringComparison.Ordinal))
        {
            return CommandResult.Failure(
                $"Workflow '{workflowExecutionId}' is not in a resumable state (status: {state.Status}). " +
                "Resume is only valid from Failed.");
        }

        // Reconstruct the definition the SAME way ExecuteWorkflowAsync does so
        // step IDs are bit-identical between start and resume runs.
        var definition = BuildDefinition(state.WorkflowName);
        if (definition is null)
        {
            return CommandResult.Failure($"No workflow registered for '{state.WorkflowName}'.");
        }

        if (state.NextStepIndex >= definition.Steps.Count)
        {
            return CommandResult.Failure(
                $"Workflow '{workflowExecutionId}' has no remaining steps to resume (cursor {state.NextStepIndex} of {definition.Steps.Count}).");
        }

        // Resume context: WorkflowId, Payload, ExecutionHash, and StepOutputs
        // come from the replayed event stream;
        // CorrelationId/IdentityId/PolicyDecision come from the live
        // CommandContext. Payload may round-trip as JsonElement when the
        // event store is Postgres-backed (typed-payload caveat tracked in
        // claude/new-rules/20260407-230000-workflow-resume-payload-and-test-coverage.md).
        var executionContext = new WorkflowExecutionContext
        {
            WorkflowId = workflowExecutionId,
            CorrelationId = context.CorrelationId,
            WorkflowName = state.WorkflowName,
            Payload = state.Payload ?? new object(),
            CurrentStepIndex = state.NextStepIndex,
            ExecutionHash = state.ExecutionHash,
            IdentityId = context.IdentityId,
            PolicyDecision = context.PolicyDecisionHash
        };

        // StepOutputs is init-only on the property; populate the existing
        // dictionary instance member-by-member.
        foreach (var kvp in state.StepOutputs)
        {
            executionContext.StepOutputs[kvp.Key] = kvp.Value;
        }

        // H8 — emit the WorkflowExecutionResumedEvent BEFORE the engine begins
        // executing so the persisted event ordering is:
        //   ... Failed → Resumed → StepCompleted → ... → Completed
        // phase1.6-S1.2: the event is constructed by the replay service via
        // WorkflowLifecycleEventFactory.Resumed (which enforces the Failed-only
        // invariant) — the aggregate is no longer mutated. We add the event to
        // the engine context's accumulated events so it persists in the same
        // fabric pass as the new step events.
        var resumedEvent = await _replayService.ResumeAsync(workflowExecutionId);
        executionContext.AccumulatedEvents.Add(resumedEvent);

        // T1MWorkflowEngine.ExecuteAsync honors context.CurrentStepIndex as the
        // resume cursor and gates the Started event on startIndex == 0, so a
        // non-zero cursor produces a clean continuation: no Started re-emit,
        // loop runs from cursor through end, factory produces StepCompleted +
        // Completed/Failed events as normal.
        // phase1.5-S5.2.3 / TC-7: forward CT into the workflow engine so the
        // execution-level / per-step linked CTSs honor the upstream token.
        var result = await _workflowEngine.ExecuteAsync(definition, executionContext, cancellationToken);

        var events = executionContext.AccumulatedEvents.AsReadOnly();
        return result.IsSuccess
            ? CommandResult.Success(events, result.Output, eventsRequirePersistence: events.Count > 0)
            : CommandResult.Success(events, eventsRequirePersistence: events.Count > 0);
    }

    private WorkflowDefinition? BuildDefinition(string workflowName)
    {
        var stepTypes = _workflowRegistry.Resolve(workflowName);
        if (stepTypes is null || stepTypes.Count == 0)
        {
            return null;
        }

        return new WorkflowDefinition
        {
            Name = workflowName,
            Steps = stepTypes.Select((type, index) =>
            {
                var step = (IWorkflowStep)_serviceProvider.GetRequiredService(type);
                return new WorkflowStepDefinition
                {
                    StepId = ComputeStepId(workflowName, step.Name, index),
                    StepName = step.Name,
                    StepType = step.StepType,
                    StepHandlerType = type
                };
            }).ToList()
        };
    }

    private static string ComputeStepId(string workflowName, string stepName, int index)
    {
        var input = $"{workflowName}:{stepName}:{index}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private async Task<CommandResult> ExecuteEngineAsync(object command, CommandContext context, CancellationToken cancellationToken)
    {
        var engineType = _engineRegistry.ResolveEngine(command.GetType());
        if (engineType is null)
        {
            return CommandResult.Failure($"No engine registered for {command.GetType().Name}.");
        }

        var engine = (IEngine)_serviceProvider.GetRequiredService(engineType);

        // phase1-gate-H8b: capture the aggregate version observed at engine
        // load time so the EventFabric can forward it to the event store as
        // an optimistic concurrency assertion. Creation commands that never
        // invoke the loader leave loadedVersion null, which the fabric will
        // forward as the -1 sentinel ("no check") preserving prior behavior.
        int? loadedVersion = null;
        var engineContext = new EngineContext(
            command,
            context.AggregateId,
            async (type, aggregateId) =>
            {
                var aggregate = (AggregateRoot)Activator.CreateInstance(type, nonPublic: true)!;
                // phase1.5-S5.2.3 / TC-5: forward CT into the
                // event-store load so PostgresEventStoreAdapter
                // ExecuteReaderAsync honors cancellation.
                var events = await _eventStore.LoadEventsAsync(aggregateId, cancellationToken);
                aggregate.LoadFromHistory(events);
                loadedVersion = aggregate.Version;
                return aggregate;
            });

        await engine.ExecuteAsync(engineContext);

        if (loadedVersion is not null && context.ExpectedVersion is null)
        {
            context.ExpectedVersion = loadedVersion;
        }

        var emittedEvents = engineContext.EmittedEvents;
        if (emittedEvents.Count == 0)
        {
            return CommandResult.Success([]);
        }

        return CommandResult.Success(emittedEvents, eventsRequirePersistence: true);
    }
}
