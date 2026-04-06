namespace Whycespace.Projections.Constitutional.Policy.Version;

public sealed record VersionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
