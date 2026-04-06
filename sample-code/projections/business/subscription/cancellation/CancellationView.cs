namespace Whycespace.Projections.Business.Subscription.Cancellation;

public sealed record CancellationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
