namespace Whycespace.Shared.Contracts.Economic.Capital.Vault;

public sealed record CreateCapitalVaultCommand(
    Guid VaultId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record AddCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal TotalCapacity,
    string Currency);

public sealed record DepositToCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount);

public sealed record AllocateFromCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount);

public sealed record ReleaseToCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount);

public sealed record WithdrawFromCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount);
