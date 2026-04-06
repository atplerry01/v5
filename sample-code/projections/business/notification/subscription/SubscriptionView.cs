namespace Whycespace.Projections.Business.Notification.Subscription;

public sealed record SubscriptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
