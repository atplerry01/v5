using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;

public sealed record CreateLineItemCommand(
    Guid LineItemId,
    Guid OrderId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit) : IHasAggregateId
{
    public Guid AggregateId => LineItemId;
}

public sealed record UpdateLineItemQuantityCommand(
    Guid LineItemId,
    decimal QuantityValue,
    string QuantityUnit) : IHasAggregateId
{
    public Guid AggregateId => LineItemId;
}

public sealed record CancelLineItemCommand(Guid LineItemId) : IHasAggregateId
{
    public Guid AggregateId => LineItemId;
}
