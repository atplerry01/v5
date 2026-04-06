using Whycespace.Domain.EconomicSystem.Ledger.Ledger;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementInvariantService
{
    public SettlementInvariantResult Validate(
        IReadOnlyList<LedgerEntry> ledgerEntries,
        Money settlementAmount)
    {
        if (ledgerEntries == null || ledgerEntries.Count == 0)
            return SettlementInvariantResult.Fail("No ledger entries backing this settlement.");

        var totalDebits = Amount.Zero;
        var totalCredits = Amount.Zero;

        for (var i = 0; i < ledgerEntries.Count; i++)
        {
            var entry = ledgerEntries[i];

            if (entry.EntryType == EntryType.Debit)
                totalDebits = totalDebits.Add(entry.Amount);
            else
                totalCredits = totalCredits.Add(entry.Amount);
        }

        if (totalDebits != totalCredits)
            return SettlementInvariantResult.Fail(
                $"Ledger not balanced: debits ({totalDebits.Value}) != credits ({totalCredits.Value}).");

        if (settlementAmount.IsNegative || settlementAmount.IsZero)
            return SettlementInvariantResult.Fail("Settlement amount must be positive.");

        return SettlementInvariantResult.Success(settlementAmount);
    }
}

public sealed record SettlementInvariantResult
{
    public bool IsValid { get; }
    public Money? ValidatedAmount { get; }
    public string? Error { get; }

    private SettlementInvariantResult(bool isValid, Money? validatedAmount, string? error)
    {
        IsValid = isValid;
        ValidatedAmount = validatedAmount;
        Error = error;
    }

    public static SettlementInvariantResult Fail(string error) => new(false, null, error);
    public static SettlementInvariantResult Success(Money amount) => new(true, amount, null);
}
