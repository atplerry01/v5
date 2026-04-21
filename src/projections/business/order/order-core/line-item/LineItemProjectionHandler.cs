using Whycespace.Projections.Business.Order.OrderCore.LineItem.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderCore.LineItem;

public sealed class LineItemProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LineItemCreatedEventSchema>,
    IProjectionHandler<LineItemUpdatedEventSchema>,
    IProjectionHandler<LineItemCancelledEventSchema>
{
    private readonly PostgresProjectionStore<LineItemReadModel> _store;

    public LineItemProjectionHandler(PostgresProjectionStore<LineItemReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LineItemCreatedEventSchema e   => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemCreatedEvent",   envelope, cancellationToken),
            LineItemUpdatedEventSchema e   => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemUpdatedEvent",   envelope, cancellationToken),
            LineItemCancelledEventSchema e => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LineItemProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(LineItemCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemCreatedEvent", null, ct);

    public Task HandleAsync(LineItemUpdatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemUpdatedEvent", null, ct);

    public Task HandleAsync(LineItemCancelledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LineItemProjectionReducer.Apply(s, e), "LineItemCancelledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<LineItemReadModel, LineItemReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new LineItemReadModel { LineItemId = aggregateId };
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
