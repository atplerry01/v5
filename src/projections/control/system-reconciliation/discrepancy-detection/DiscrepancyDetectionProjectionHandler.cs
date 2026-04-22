using Whycespace.Projections.Control.SystemReconciliation.DiscrepancyDetection.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemReconciliation.DiscrepancyDetection;

public sealed class DiscrepancyDetectionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<DiscrepancyDetectedEventSchema>,
    IProjectionHandler<DiscrepancyDetectionDismissedEventSchema>
{
    private readonly PostgresProjectionStore<DiscrepancyDetectionReadModel> _store;

    public DiscrepancyDetectionProjectionHandler(PostgresProjectionStore<DiscrepancyDetectionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            DiscrepancyDetectedEventSchema e           => Project(e.AggregateId, s => DiscrepancyDetectionProjectionReducer.Apply(s, e), "DiscrepancyDetectedEvent",           envelope, cancellationToken),
            DiscrepancyDetectionDismissedEventSchema e => Project(e.AggregateId, s => DiscrepancyDetectionProjectionReducer.Apply(s, e), "DiscrepancyDetectionDismissedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"DiscrepancyDetectionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(DiscrepancyDetectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DiscrepancyDetectionProjectionReducer.Apply(s, e), "DiscrepancyDetectedEvent", null, ct);

    public Task HandleAsync(DiscrepancyDetectionDismissedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => DiscrepancyDetectionProjectionReducer.Apply(s, e), "DiscrepancyDetectionDismissedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<DiscrepancyDetectionReadModel, DiscrepancyDetectionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new DiscrepancyDetectionReadModel { DetectionId = aggregateId };
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
