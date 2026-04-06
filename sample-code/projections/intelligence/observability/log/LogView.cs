namespace Whycespace.Projections.Intelligence.Observability.Log;

public sealed record LogView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
