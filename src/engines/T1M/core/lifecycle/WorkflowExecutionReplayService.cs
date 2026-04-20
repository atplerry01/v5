using System.Text.Json;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

namespace Whycespace.Engines.T1M.Core.Lifecycle;

/// <summary>
/// Reconstructs <see cref="WorkflowExecutionAggregate"/> deterministically
/// from the event store and projects it onto the runtime-facing
/// <see cref="WorkflowExecutionReplayState"/> DTO.
///
/// This is the engine-side implementation of
/// <see cref="IWorkflowExecutionReplayService"/>; it lives under
/// src/engines/T1M/lifecycle/ so it can reference
/// Whycespace.Domain.OrchestrationSystem.Workflow.Execution.* without
/// violating runtime.guard R-DOM-01. The runtime dispatcher depends only
/// on the shared contract.
///
/// Determinism: replay folds events through the aggregate's existing
/// LoadFromHistory pathway — no clock, no Guid.NewGuid, no policy
/// re-evaluation, no sentinel writes. Protected by
/// replay-determinism.guard (REPLAY-SENTINEL-PROTECTED-01).
/// </summary>
public sealed class WorkflowExecutionReplayService : IWorkflowExecutionReplayService
{
    private readonly IEventStore _eventStore;
    private readonly IPayloadTypeRegistry _payloadTypes;
    private readonly WorkflowLifecycleEventFactory _lifecycleFactory;

    public WorkflowExecutionReplayService(
        IEventStore eventStore,
        IPayloadTypeRegistry payloadTypes,
        WorkflowLifecycleEventFactory lifecycleFactory)
    {
        _eventStore = eventStore;
        _payloadTypes = payloadTypes;
        _lifecycleFactory = lifecycleFactory;
    }

    public async Task<WorkflowExecutionReplayState?> ReplayAsync(Guid workflowExecutionId)
    {
        var events = await _eventStore.LoadEventsAsync(workflowExecutionId);
        if (events.Count == 0)
        {
            return null;
        }

        var aggregate = (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate),
            nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        // NextStepIndex = number of completed step events. Unambiguous and
        // event-derived; do not use aggregate.CurrentStepIndex which collapses
        // "started, no steps" and "step 0 completed" to the same value.
        // Payload + StepOutputs are extracted from the same pass.
        //
        // Typed-payload rehydration (rule E-TYPE-03): when an event was
        // round-tripped through PostgresEventStoreAdapter, the static
        // `object?` typing causes Payload/Output to come back as JsonElement.
        // If the event also carries a PayloadType/OutputType discriminator
        // (stamped by WorkflowLifecycleEventFactory at write time, see
        // E-TYPE-02), we re-deserialize via IPayloadTypeRegistry to restore
        // the original CLR type. In-process / in-memory replay preserves the
        // original CLR reference and skips the JsonElement branch.
        var nextStepIndex = 0;
        object? payload = null;
        var stepOutputs = new Dictionary<string, object?>();
        for (var i = 0; i < events.Count; i++)
        {
            switch (events[i])
            {
                case WorkflowExecutionStartedEvent started:
                    payload = Rehydrate(started.Payload, started.PayloadType);
                    break;
                case WorkflowStepCompletedEvent step:
                    nextStepIndex++;
                    stepOutputs[step.StepName] = Rehydrate(step.Output, step.OutputType);
                    break;
            }
        }

        return new WorkflowExecutionReplayState(
            WorkflowExecutionId: aggregate.Id.Value,
            WorkflowName: aggregate.WorkflowName,
            NextStepIndex: nextStepIndex,
            ExecutionHash: aggregate.ExecutionHash,
            Status: aggregate.Status.ToString(),
            Payload: payload,
            StepOutputs: stepOutputs);
    }

    public async Task<object> ResumeAsync(Guid workflowExecutionId)
    {
        var events = await _eventStore.LoadEventsAsync(workflowExecutionId);
        if (events.Count == 0)
            throw new InvalidOperationException(
                $"Workflow execution '{workflowExecutionId}' has no recorded events; cannot resume.");

        var aggregate = (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate),
            nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        // R3.A.6 D8: this service method enforces the Failed-only
        // precondition for the failure-retry caller. Suspended-resume
        // for the human-approval path is handled by
        // ResumeWithApprovalAsync per R-WF-APPROVAL-03. The factory's
        // Resumed(aggregate) overload accepts both Failed and Suspended
        // (R-WORKFLOW-SUSPEND-RESUME-GUARD-01); the narrower check here
        // reflects failure-retry intent, not a factory constraint.
        if (aggregate.Status != WorkflowExecutionStatus.Failed)
            throw new InvalidOperationException(
                $"Workflow execution '{workflowExecutionId}' is in status {aggregate.Status}; " +
                "failure-retry resume is only valid from the Failed state. " +
                "Use ResumeWithApprovalAsync for Suspended-for-approval workflows.");

        // phase1.6-S1.2 (E-LIFECYCLE-FACTORY-CALL-SITE-01): construct the
        // resume event via the lifecycle factory instead of mutating the
        // aggregate. The factory re-validates the Failed precondition, reads
        // the failure context from the aggregate's public surface, and
        // returns the event without touching aggregate state. The runtime
        // dispatcher then appends it to the workflow's accumulated events,
        // the persist pipeline writes it to the event store, and the next
        // replay reconstructs the Running status via Apply.
        return _lifecycleFactory.Resumed(aggregate);
    }

    /// <inheritdoc />
    public async Task<object> ResumeWithApprovalAsync(
        Guid workflowExecutionId, string approverIdentity, string? rationale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverIdentity);

        var (aggregate, latestSuspended) = await LoadForApprovalAsync(
            workflowExecutionId, failureMessage: WorkflowApprovalErrors.CannotApproveUnlessAwaitingApproval);

        var signal = ExtractSignalSuffix(latestSuspended.Reason, WorkflowApprovalErrors.HumanApprovalPrefix);
        var grantedCarrier = ComposeApprovalCarrier(
            WorkflowApprovalErrors.HumanApprovalGrantedPrefix, signal, approverIdentity, rationale);

        return _lifecycleFactory.Resumed(aggregate, grantedCarrier);
    }

    /// <inheritdoc />
    public async Task<object> CancelSuspendedAsync(
        Guid workflowExecutionId, string approverIdentity, string? rationale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approverIdentity);

        var (_, latestSuspended) = await LoadForApprovalAsync(
            workflowExecutionId, failureMessage: WorkflowApprovalErrors.CannotRejectUnlessAwaitingApproval);

        var signal = ExtractSignalSuffix(latestSuspended.Reason, WorkflowApprovalErrors.HumanApprovalPrefix);
        var rejectedCarrier = ComposeApprovalCarrier(
            WorkflowApprovalErrors.HumanApprovalRejectedPrefix, signal, approverIdentity, rationale);

        return _lifecycleFactory.Cancelled(
            workflowExecutionId, latestSuspended.StepName, rejectedCarrier);
    }

    private async Task<(WorkflowExecutionAggregate, WorkflowExecutionSuspendedEvent)> LoadForApprovalAsync(
        Guid workflowExecutionId, string failureMessage)
    {
        var events = await _eventStore.LoadEventsAsync(workflowExecutionId);
        if (events.Count == 0)
            throw new InvalidOperationException(
                $"Workflow execution '{workflowExecutionId}' has no recorded events; cannot resolve approval.");

        var aggregate = (WorkflowExecutionAggregate)System.Activator.CreateInstance(
            typeof(WorkflowExecutionAggregate),
            nonPublic: true)!;
        aggregate.LoadFromHistory(events);

        if (aggregate.Status != WorkflowExecutionStatus.Suspended)
            throw new InvalidOperationException(failureMessage);

        var latestSuspended = FindLatestSuspendedEvent(events);
        if (latestSuspended is null)
            throw new InvalidOperationException(failureMessage);

        // R-WF-APPROVAL-02: reject when the carrier prefix does not
        // denote human-approval (timer/external-dep suspends etc.).
        if (!IsHumanApprovalSignal(latestSuspended.Reason))
            throw new InvalidOperationException(failureMessage);

        return (aggregate, latestSuspended);
    }

    private static WorkflowExecutionSuspendedEvent? FindLatestSuspendedEvent(IReadOnlyList<object> events)
    {
        for (var i = events.Count - 1; i >= 0; i--)
        {
            if (events[i] is WorkflowExecutionSuspendedEvent suspended)
                return suspended;
        }
        return null;
    }

    private static bool IsHumanApprovalSignal(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return false;
        // Exact match on the prefix word. Either "human_approval" on its
        // own or "human_approval:<signal>..." qualifies; "human_approval_"
        // does NOT match (prevents accidental matches against the
        // granted/rejected variants on a malformed carrier).
        if (reason == WorkflowApprovalErrors.HumanApprovalPrefix) return true;
        return reason.StartsWith(WorkflowApprovalErrors.HumanApprovalPrefix + ":", StringComparison.Ordinal);
    }

    private static string? ExtractSignalSuffix(string reason, string prefix)
    {
        if (string.IsNullOrEmpty(reason)) return null;
        var prefixWithColon = prefix + ":";
        if (!reason.StartsWith(prefixWithColon, StringComparison.Ordinal)) return null;
        var suffix = reason[prefixWithColon.Length..];
        return string.IsNullOrEmpty(suffix) ? null : suffix;
    }

    private static string ComposeApprovalCarrier(
        string prefix, string? signal, string actor, string? rationale)
    {
        // Canonical shape:
        //   {prefix}:{signal?}:{actor}[:{rationale}]
        // Signal may be null when the original Suspended carrier was
        // the bare "human_approval" prefix without a suffix. Empty
        // signal segment is preserved positionally so consumers can
        // parse by index. Rationale is optional trailing context.
        var actorSegment = actor ?? string.Empty;
        var signalSegment = signal ?? string.Empty;
        var baseCarrier = $"{prefix}:{signalSegment}:{actorSegment}";
        return string.IsNullOrEmpty(rationale) ? baseCarrier : $"{baseCarrier}:{rationale}";
    }

    private object? Rehydrate(object? value, string? typeName)
    {
        // Null payloads remain null — there is nothing to type and no
        // JsonElement to leak.
        if (value is null) return null;

        // In-memory / in-process replay: the value is already a typed CLR
        // object. No rehydration is required and a missing typeName is
        // tolerated because there is no JsonElement crossing the replay
        // boundary.
        if (value is not JsonElement json) return value;

        // Postgres-backed replay: the value came back as a JsonElement.
        // H10 strict mode — the typeName MUST be present and resolvable.
        // No silent fallback, no dynamic, no JsonElement leakage past this
        // boundary. (See claude/project-prompts/20260407-215052-orchestration-
        // system-h10-type-safety.md.)
        if (typeName is null)
            throw new InvalidOperationException(
                "H10 type safety violation: workflow event payload arrived as JsonElement " +
                "but carries no PayloadType/OutputType discriminator. " +
                "Register the CLR type via IPayloadTypeRegistry from the owning " +
                "IDomainBootstrapModule so the write-side factory stamps a discriminator.");

        var type = _payloadTypes.Resolve(typeName);
        return JsonSerializer.Deserialize(json.GetRawText(), type);
    }
}
