namespace Whycespace.Projections.Business.Logistic.Tracking;

public sealed record TrackingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
