namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class LedgerInvariantService
{
    private readonly BalancedLedgerSpecification _balancedSpec = new();

    public LedgerInvariantResult Validate(IReadOnlyList<DebitCredit> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        if (entries.Count == 0)
            return LedgerInvariantResult.Failure("Entry list cannot be empty.");

        if (!_balancedSpec.IsSatisfiedBy(entries))
        {
            var totalDebits = entries.Sum(e => e.Debit);
            var totalCredits = entries.Sum(e => e.Credit);
            return LedgerInvariantResult.Failure(
                $"Ledger imbalance: total debits ({totalDebits}) do not equal total credits ({totalCredits}).");
        }

        var balancedTotal = entries.Sum(e => e.Debit);
        return LedgerInvariantResult.Success(balancedTotal);
    }
}

public sealed record LedgerInvariantResult
{
    public bool IsValid { get; }
    public decimal BalancedTotal { get; }
    public string? Error { get; }

    private LedgerInvariantResult(bool isValid, decimal balancedTotal, string? error)
    {
        IsValid = isValid;
        BalancedTotal = balancedTotal;
        Error = error;
    }

    public static LedgerInvariantResult Success(decimal balancedTotal) => new(true, balancedTotal, null);
    public static LedgerInvariantResult Failure(string error) => new(false, 0m, error);
}
