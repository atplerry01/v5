namespace Whycespace.Projections.Decision.Risk.Alert;

public sealed record AlertView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
