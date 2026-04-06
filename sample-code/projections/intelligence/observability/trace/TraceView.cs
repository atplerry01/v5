namespace Whycespace.Projections.Intelligence.Observability.Trace;

public sealed record TraceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
