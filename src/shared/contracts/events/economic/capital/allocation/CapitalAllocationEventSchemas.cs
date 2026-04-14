namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Allocation;

public sealed record AllocationCreatedEventSchema(
    Guid AggregateId,
    Guid SourceAccountId,
    Guid TargetId,
    decimal Amount,
    string Currency,
    DateTimeOffset AllocatedAt);

public sealed record AllocationReleasedEventSchema(
    Guid AggregateId,
    Guid SourceAccountId,
    decimal Amount,
    DateTimeOffset ReleasedAt);

public sealed record AllocationCompletedEventSchema(
    Guid AggregateId,
    DateTimeOffset CompletedAt);

public sealed record CapitalAllocatedToSpvEventSchema(
    Guid AggregateId,
    string SpvTargetId,
    decimal OwnershipPercentage);
