namespace Whycespace.Projections.Business.Subscription.Usage;

public sealed record UsageView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
