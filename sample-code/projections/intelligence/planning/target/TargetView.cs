namespace Whycespace.Projections.Intelligence.Planning.Target;

public sealed record TargetView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
