namespace Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;

public sealed record AllocationReadModel
{
    public Guid AllocationId { get; init; }
    public Guid ResourceId { get; init; }
    public int RequestedCapacity { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
