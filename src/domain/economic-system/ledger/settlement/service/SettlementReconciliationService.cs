namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementReconciliationService
{
    public bool ValidateJournalLink(SettlementAggregate settlement) =>
        settlement.JournalId != Guid.Empty && settlement.ObligationId != Guid.Empty;
}
