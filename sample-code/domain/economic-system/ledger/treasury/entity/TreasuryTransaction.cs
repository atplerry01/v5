namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed class TreasuryTransaction
{
    public Guid Id { get; }

    public TreasuryTransaction(Guid id)
    {
        Id = id;
    }
}
