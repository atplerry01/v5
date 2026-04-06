namespace Whycespace.Projections.Core.State.StateVersion;

public sealed record StateVersionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
