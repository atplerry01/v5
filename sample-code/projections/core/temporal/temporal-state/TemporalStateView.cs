namespace Whycespace.Projections.Core.Temporal.TemporalState;

public sealed record TemporalStateView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
