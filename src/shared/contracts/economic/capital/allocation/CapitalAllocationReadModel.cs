namespace Whycespace.Shared.Contracts.Economic.Capital.Allocation;

public sealed record CapitalAllocationReadModel
{
    public Guid AllocationId { get; init; }
    public Guid SourceAccountId { get; init; }
    public Guid TargetId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset AllocatedAt { get; init; }
    public string? TargetType { get; init; }
    public string? SpvTargetId { get; init; }
    public decimal? OwnershipPercentage { get; init; }
}
