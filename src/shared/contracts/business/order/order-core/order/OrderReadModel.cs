namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;

public sealed record OrderReadModel
{
    public Guid OrderId { get; init; }
    public Guid SourceReferenceId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
}
