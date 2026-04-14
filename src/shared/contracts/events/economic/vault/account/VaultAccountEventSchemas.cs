namespace Whycespace.Shared.Contracts.Events.Economic.Vault.Account;

// Contract-layer mirror of the vault Slice enum. Shared contracts cannot
// reference the domain assembly; integer values MUST match
// Whycespace.Domain.EconomicSystem.Vault.Slice.SliceType.
public enum VaultSliceType
{
    Slice1 = 1,
    Slice2 = 2,
    Slice3 = 3,
    Slice4 = 4
}

public sealed record SpvProfitReceivedEventSchema(
    Guid AggregateId,
    decimal Amount,
    string Currency,
    VaultSliceType Slice);

public sealed record VaultDebitedEventSchema(
    Guid AggregateId,
    decimal Amount,
    VaultSliceType Slice);

public sealed record VaultCreditedEventSchema(
    Guid AggregateId,
    decimal Amount,
    VaultSliceType Slice);

public sealed record VaultFundedEventSchema(
    Guid AggregateId,
    decimal Amount,
    string Currency);

public sealed record CapitalAllocatedToSliceEventSchema(
    Guid AggregateId,
    decimal Amount,
    VaultSliceType FromSlice,
    VaultSliceType ToSlice);
