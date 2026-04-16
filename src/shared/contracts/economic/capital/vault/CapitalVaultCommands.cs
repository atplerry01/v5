using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Vault;

public sealed record CreateCapitalVaultCommand(
    Guid VaultId,
    Guid OwnerId,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}

public sealed record AddCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal TotalCapacity,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}

public sealed record DepositToCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}

public sealed record AllocateFromCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}

public sealed record ReleaseToCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}

public sealed record WithdrawFromCapitalVaultSliceCommand(
    Guid VaultId,
    Guid SliceId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultId;
}
