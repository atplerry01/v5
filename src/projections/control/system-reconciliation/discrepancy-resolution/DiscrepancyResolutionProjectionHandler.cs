using Whycespace.Projections.Control.SystemReconciliation.DiscrepancyResolution.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemReconciliation.DiscrepancyResolution;

public sealed class DiscrepancyResolutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DiscrepancyResolutionInitiatedEventSchema>,
    IProjectionHandler<DiscrepancyResolutionCompletedEventSchema>
{
    private readonly PostgresProjectionStore<DiscrepancyResolutionReadModel> _store;

    public DiscrepancyResolutionProjectionHandler(PostgresProjectionStore<DiscrepancyResolutionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DiscrepancyResolutionInitiatedEventSchema e  => Project(e.AggregateId, s => DiscrepancyResolutionProjectionReducer.Apply(s, e), "DiscrepancyResolutionInitiatedEvent",  envelope, cancellationToken),
            DiscrepancyResolutionCompletedEventSchema e  => Project(e.AggregateId, s => DiscrepancyResolutionProjectionReducer.Apply(s, e), "DiscrepancyResolutionCompletedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DiscrepancyResolutionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DiscrepancyResolutionInitiatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DiscrepancyResolutionProjectionReducer.Apply(s, e), "DiscrepancyResolutionInitiatedEvent", null, ct);

    public Task HandleAsync(DiscrepancyResolutionCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DiscrepancyResolutionProjectionReducer.Apply(s, e), "DiscrepancyResolutionCompletedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DiscrepancyResolutionReadModel, DiscrepancyResolutionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DiscrepancyResolutionReadModel { ResolutionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
