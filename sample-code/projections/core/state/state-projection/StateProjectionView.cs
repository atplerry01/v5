namespace Whycespace.Projections.Core.State.StateProjection;

public sealed record StateProjectionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
