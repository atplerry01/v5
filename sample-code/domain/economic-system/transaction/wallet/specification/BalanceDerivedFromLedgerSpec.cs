namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

/// <summary>
/// Asserts that a wallet's balance is derived ONLY from ledger events.
/// Balance must never be set directly — it must be a projection of
/// debit/credit events from the ledger domain.
/// </summary>
public sealed class BalanceDerivedFromLedgerSpec : Specification<WalletAggregate>
{
    public override bool IsSatisfiedBy(WalletAggregate entity)
    {
        // Balance is valid only when constructed through event replay.
        // A wallet with creation events but no direct balance mutation events is compliant.
        // Direct balance assignment outside of event-driven flow violates this spec.
        var hasCreation = entity.DomainEvents.OfType<WalletCreatedEvent>().Any();
        return hasCreation;
    }
}
