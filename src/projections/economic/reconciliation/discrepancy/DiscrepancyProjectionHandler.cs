using Whycespace.Projections.Economic.Reconciliation.Discrepancy.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Reconciliation.Discrepancy;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Reconciliation.Discrepancy;

public sealed class DiscrepancyProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DiscrepancyDetectedEventSchema>,
    IProjectionHandler<DiscrepancyInvestigatedEventSchema>,
    IProjectionHandler<DiscrepancyResolvedEventSchema>
{
    private readonly PostgresProjectionStore<DiscrepancyReadModel> _store;

    public DiscrepancyProjectionHandler(PostgresProjectionStore<DiscrepancyReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DiscrepancyDetectedEventSchema     e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            DiscrepancyInvestigatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            DiscrepancyResolvedEventSchema     e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DiscrepancyProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DiscrepancyDetectedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public Task HandleAsync(DiscrepancyInvestigatedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public Task HandleAsync(DiscrepancyResolvedEventSchema e, CancellationToken ct = default)
        => Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(DiscrepancyDetectedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.DiscrepancyId, ct) ?? new DiscrepancyReadModel { DiscrepancyId = e.DiscrepancyId };
        state = DiscrepancyProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.DiscrepancyId, state, "DiscrepancyDetectedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(DiscrepancyInvestigatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.DiscrepancyId, ct) ?? new DiscrepancyReadModel { DiscrepancyId = e.DiscrepancyId };
        state = DiscrepancyProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.DiscrepancyId, state, "DiscrepancyInvestigatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(DiscrepancyResolvedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.DiscrepancyId, ct) ?? new DiscrepancyReadModel { DiscrepancyId = e.DiscrepancyId };
        state = DiscrepancyProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.DiscrepancyId, state, "DiscrepancyResolvedEvent", eventId, eventVersion, correlationId, ct);
    }
}
