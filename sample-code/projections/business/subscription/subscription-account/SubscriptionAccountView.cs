namespace Whycespace.Projections.Business.Subscription.SubscriptionAccount;

public sealed record SubscriptionAccountView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
