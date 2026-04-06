namespace Whycespace.Projections.Business.Integration.Subscription;

public sealed record SubscriptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
