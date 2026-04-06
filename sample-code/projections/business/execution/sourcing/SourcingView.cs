namespace Whycespace.Projections.Business.Execution.Sourcing;

public sealed record SourcingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
