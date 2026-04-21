namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;

public sealed record LineItemReadModel
{
    public Guid LineItemId { get; init; }
    public Guid OrderId { get; init; }
    public int SubjectKind { get; init; }
    public Guid SubjectId { get; init; }
    public decimal QuantityValue { get; init; }
    public string QuantityUnit { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
