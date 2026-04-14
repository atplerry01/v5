namespace Whycespace.Shared.Contracts.Economic.Capital.Allocation;

public sealed record CreateCapitalAllocationCommand(
    Guid AllocationId,
    Guid SourceAccountId,
    Guid TargetId,
    decimal Amount,
    string Currency,
    DateTimeOffset AllocatedAt);

public sealed record ReleaseCapitalAllocationCommand(
    Guid AllocationId,
    DateTimeOffset ReleasedAt);

public sealed record CompleteCapitalAllocationCommand(
    Guid AllocationId,
    DateTimeOffset CompletedAt);

public sealed record AllocateCapitalToSpvCommand(
    Guid AllocationId,
    string SpvTargetId,
    decimal OwnershipPercentage);
