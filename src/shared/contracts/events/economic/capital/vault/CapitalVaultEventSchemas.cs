namespace Whycespace.Shared.Contracts.Events.Economic.Capital.Vault;

public sealed record VaultCreatedEventSchema(
    Guid AggregateId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record VaultSliceCreatedEventSchema(
    Guid AggregateId,
    Guid SliceId,
    decimal TotalCapacity,
    string Currency);

public sealed record CapitalDepositedEventSchema(
    Guid AggregateId,
    Guid SliceId,
    decimal DepositedAmount,
    decimal NewSliceCapacity,
    decimal NewVaultTotal);

public sealed record CapitalAllocatedToSliceEventSchema(
    Guid AggregateId,
    Guid SliceId,
    decimal AllocatedAmount,
    decimal NewSliceAvailable,
    decimal NewSliceUsed);

public sealed record CapitalReleasedFromSliceEventSchema(
    Guid AggregateId,
    Guid SliceId,
    decimal ReleasedAmount,
    decimal NewSliceAvailable,
    decimal NewSliceUsed);

public sealed record CapitalWithdrawnEventSchema(
    Guid AggregateId,
    Guid SliceId,
    decimal WithdrawnAmount,
    decimal NewSliceCapacity,
    decimal NewVaultTotal);
