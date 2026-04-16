using Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Process;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Projections.Economic.Reconciliation.Workflow;

/// <summary>
/// Materialises the reconciliation lifecycle workflow read model. One row per
/// process aggregate; a single <c>discrepancy_id</c> column links the process
/// to its currently-open discrepancy so the lifecycle handler can close the
/// loop on <c>DiscrepancyResolvedEvent</c>.
///
/// Event fan-in:
///   Process side:     ReconciliationTriggered / Matched / Mismatched / Resolved
///   Discrepancy side: DiscrepancyDetected / Investigated / Resolved
///
/// Non-domain: this handler only writes to the projection store; no aggregate
/// access, no command dispatch.
/// </summary>
public sealed class ReconciliationWorkflowProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReconciliationTriggeredEventSchema>,
    IProjectionHandler<ReconciliationMatchedEventSchema>,
    IProjectionHandler<ReconciliationMismatchedEventSchema>,
    IProjectionHandler<ReconciliationResolvedEventSchema>,
    IProjectionHandler<DiscrepancyDetectedEventSchema>,
    IProjectionHandler<DiscrepancyInvestigatedEventSchema>,
    IProjectionHandler<DiscrepancyResolvedEventSchema>
{
    private readonly IReconciliationWorkflowStore _store;
    private readonly IClock _clock;

    public ReconciliationWorkflowProjectionHandler(
        IReconciliationWorkflowStore store,
        IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ReconciliationTriggeredEventSchema  e => HandleAsync(e, envelope.CorrelationId, cancellationToken),
            ReconciliationMatchedEventSchema    e => HandleAsync(e, cancellationToken),
            ReconciliationMismatchedEventSchema e => HandleAsync(e, cancellationToken),
            ReconciliationResolvedEventSchema   e => HandleAsync(e, cancellationToken),
            DiscrepancyDetectedEventSchema      e => HandleAsync(e, cancellationToken),
            DiscrepancyInvestigatedEventSchema  e => HandleAsync(e, cancellationToken),
            DiscrepancyResolvedEventSchema      e => HandleAsync(e, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ReconciliationWorkflowProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ReconciliationTriggeredEventSchema e, CancellationToken ct = default)
        => HandleAsync(e, correlationId: Guid.Empty, ct);

    private Task HandleAsync(ReconciliationTriggeredEventSchema e, Guid correlationId, CancellationToken ct)
        => _store.UpsertByProcessAsync(new ReconciliationWorkflowReadModel
        {
            ProcessId     = e.ProcessId,
            CurrentState  = ReconciliationLifecycleState.Triggered.ToString(),
            LastEvent     = "ReconciliationTriggeredEvent",
            CorrelationId = correlationId,
            UpdatedAt     = _clock.UtcNow
        }, ct);

    public Task HandleAsync(ReconciliationMatchedEventSchema e, CancellationToken ct = default)
        => _store.UpsertByProcessAsync(new ReconciliationWorkflowReadModel
        {
            ProcessId    = e.ProcessId,
            CurrentState = ReconciliationLifecycleState.Matched.ToString(),
            LastEvent    = "ReconciliationMatchedEvent",
            UpdatedAt    = _clock.UtcNow
        }, ct);

    public Task HandleAsync(ReconciliationMismatchedEventSchema e, CancellationToken ct = default)
        => _store.UpsertByProcessAsync(new ReconciliationWorkflowReadModel
        {
            ProcessId    = e.ProcessId,
            CurrentState = ReconciliationLifecycleState.Mismatched.ToString(),
            LastEvent    = "ReconciliationMismatchedEvent",
            UpdatedAt    = _clock.UtcNow
        }, ct);

    public Task HandleAsync(ReconciliationResolvedEventSchema e, CancellationToken ct = default)
        => _store.UpsertByProcessAsync(new ReconciliationWorkflowReadModel
        {
            ProcessId    = e.ProcessId,
            CurrentState = ReconciliationLifecycleState.Resolved.ToString(),
            LastEvent    = "ReconciliationResolvedEvent",
            UpdatedAt    = _clock.UtcNow
        }, ct);

    public Task HandleAsync(DiscrepancyDetectedEventSchema e, CancellationToken ct = default)
        => _store.UpsertByProcessAsync(new ReconciliationWorkflowReadModel
        {
            ProcessId     = e.ProcessReference,
            DiscrepancyId = e.DiscrepancyId,
            CurrentState  = ReconciliationLifecycleState.Investigating.ToString(),
            LastEvent     = "DiscrepancyDetectedEvent",
            UpdatedAt     = _clock.UtcNow
        }, ct);

    public Task HandleAsync(DiscrepancyInvestigatedEventSchema e, CancellationToken ct = default)
        => _store.UpdateStateByDiscrepancyAsync(
            e.DiscrepancyId,
            ReconciliationLifecycleState.ResolvingDiscrepancy.ToString(),
            "DiscrepancyInvestigatedEvent",
            _clock.UtcNow,
            ct);

    public Task HandleAsync(DiscrepancyResolvedEventSchema e, CancellationToken ct = default)
        => _store.UpdateStateByDiscrepancyAsync(
            e.DiscrepancyId,
            ReconciliationLifecycleState.ResolvingDiscrepancy.ToString(),
            "DiscrepancyResolvedEvent",
            _clock.UtcNow,
            ct);
}
