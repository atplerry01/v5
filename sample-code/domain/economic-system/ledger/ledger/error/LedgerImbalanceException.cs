namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed class LedgerImbalanceException : DomainException
{
    public LedgerImbalanceException(string message)
        : base("LEDGER_IMBALANCE", message) { }
}