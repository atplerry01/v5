using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Vault.Account;

public sealed record CreateVaultAccountCommand(
    Guid VaultAccountId,
    Guid OwnerSubjectId,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}

public sealed record FundVaultCommand(
    Guid VaultAccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}

public sealed record InvestCommand(
    Guid VaultAccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}

public sealed record ApplyRevenueCommand(
    Guid VaultAccountId,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}

public sealed record DebitSliceCommand(
    Guid VaultAccountId,
    VaultSliceType Slice,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}

public sealed record CreditSliceCommand(
    Guid VaultAccountId,
    VaultSliceType Slice,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => VaultAccountId;
}
