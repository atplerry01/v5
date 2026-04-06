namespace Whycespace.Projections.Business.Notification.Delivery;

public sealed record DeliveryView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
