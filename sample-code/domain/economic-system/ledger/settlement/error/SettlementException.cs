namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementException : DomainException
{
    public SettlementException(string message)
        : base("SETTLEMENT_INVARIANT_VIOLATION", message) { }
}
