namespace Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;

public sealed record CancellationReadModel
{
    public Guid CancellationId { get; init; }
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
