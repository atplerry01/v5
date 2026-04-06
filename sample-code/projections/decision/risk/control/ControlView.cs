namespace Whycespace.Projections.Decision.Risk.Control;

public sealed record ControlView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
