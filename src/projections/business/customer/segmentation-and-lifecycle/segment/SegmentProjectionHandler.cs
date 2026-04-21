using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Segment.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed class SegmentProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SegmentCreatedEventSchema>,
    IProjectionHandler<SegmentUpdatedEventSchema>,
    IProjectionHandler<SegmentActivatedEventSchema>,
    IProjectionHandler<SegmentArchivedEventSchema>
{
    private readonly PostgresProjectionStore<SegmentReadModel> _store;

    public SegmentProjectionHandler(PostgresProjectionStore<SegmentReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            SegmentCreatedEventSchema e   => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentCreatedEvent",   envelope, cancellationToken),
            SegmentUpdatedEventSchema e   => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentUpdatedEvent",   envelope, cancellationToken),
            SegmentActivatedEventSchema e => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentActivatedEvent", envelope, cancellationToken),
            SegmentArchivedEventSchema e  => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentArchivedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SegmentProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(SegmentCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentCreatedEvent", null, ct);

    public Task HandleAsync(SegmentUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentUpdatedEvent", null, ct);

    public Task HandleAsync(SegmentActivatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentActivatedEvent", null, ct);

    public Task HandleAsync(SegmentArchivedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => SegmentProjectionReducer.Apply(s, e), "SegmentArchivedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<SegmentReadModel, SegmentReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new SegmentReadModel { SegmentId = aggregateId };
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
