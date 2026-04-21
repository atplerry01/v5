using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;

public sealed record CreateOrderCommand(
    Guid OrderId,
    Guid SourceReferenceId,
    string Description) : IHasAggregateId
{
    public Guid AggregateId => OrderId;
}

public sealed record ConfirmOrderCommand(Guid OrderId) : IHasAggregateId
{
    public Guid AggregateId => OrderId;
}

public sealed record CompleteOrderCommand(Guid OrderId) : IHasAggregateId
{
    public Guid AggregateId => OrderId;
}

public sealed record CancelOrderCommand(
    Guid OrderId,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => OrderId;
}
