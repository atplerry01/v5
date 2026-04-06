namespace Whycespace.Projections.Business.Integration.Replay;

public sealed record ReplayView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
