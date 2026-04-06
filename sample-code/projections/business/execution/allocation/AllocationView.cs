namespace Whycespace.Projections.Business.Execution.Allocation;

public sealed record AllocationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
