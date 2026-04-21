using Whycespace.Projections.Business.Order.OrderCore.Order.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderCore.Order;

public sealed class OrderProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<OrderCreatedEventSchema>,
    IProjectionHandler<OrderConfirmedEventSchema>,
    IProjectionHandler<OrderCompletedEventSchema>,
    IProjectionHandler<OrderCancelledEventSchema>
{
    private readonly PostgresProjectionStore<OrderReadModel> _store;

    public OrderProjectionHandler(PostgresProjectionStore<OrderReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            OrderCreatedEventSchema e   => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCreatedEvent",   envelope, cancellationToken),
            OrderConfirmedEventSchema e => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderConfirmedEvent", envelope, cancellationToken),
            OrderCompletedEventSchema e => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCompletedEvent", envelope, cancellationToken),
            OrderCancelledEventSchema e => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCancelledEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"OrderProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(OrderCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCreatedEvent", null, ct);

    public Task HandleAsync(OrderConfirmedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderConfirmedEvent", null, ct);

    public Task HandleAsync(OrderCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCompletedEvent", null, ct);

    public Task HandleAsync(OrderCancelledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => OrderProjectionReducer.Apply(s, e), "OrderCancelledEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<OrderReadModel, OrderReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new OrderReadModel { OrderId = aggregateId };
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
