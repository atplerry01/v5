using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Events.Economic.Vault.Account;

namespace Whycespace.Projections.Economic.Vault.Account.Reducer;

/// <summary>
/// Reducer for the vault account read model. Tracks Total/Free/Locked/Invested
/// balances per vault, recomputed from the event stream. Not authoritative —
/// the domain aggregate's VaultMetrics is the write-side source of truth;
/// this read model is the query-side materialization.
/// </summary>
public static class VaultAccountProjectionReducer
{
    public static VaultAccountReadModel Apply(VaultAccountReadModel state, VaultAccountCreatedEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            Currency = e.Currency
        };

    public static VaultAccountReadModel Apply(VaultAccountReadModel state, SpvProfitReceivedEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            Currency = e.Currency,
            TotalBalance = state.TotalBalance + e.Amount,
            FreeBalance = state.FreeBalance + e.Amount
        };

    public static VaultAccountReadModel Apply(VaultAccountReadModel state, VaultFundedEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            Currency = e.Currency,
            TotalBalance = state.TotalBalance + e.Amount,
            FreeBalance = state.FreeBalance + e.Amount
        };

    public static VaultAccountReadModel Apply(VaultAccountReadModel state, VaultDebitedEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            TotalBalance = state.TotalBalance - e.Amount,
            FreeBalance = state.FreeBalance - e.Amount
        };

    public static VaultAccountReadModel Apply(VaultAccountReadModel state, VaultCreditedEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            TotalBalance = state.TotalBalance + e.Amount,
            FreeBalance = state.FreeBalance + e.Amount
        };

    public static VaultAccountReadModel Apply(VaultAccountReadModel state, CapitalAllocatedToSliceEventSchema e) =>
        state with
        {
            VaultAccountId = e.AggregateId,
            FreeBalance = state.FreeBalance - e.Amount,
            InvestedBalance = state.InvestedBalance + e.Amount
        };
}
