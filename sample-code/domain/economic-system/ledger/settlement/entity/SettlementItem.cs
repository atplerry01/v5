namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed class SettlementItem
{
    public Guid Id { get; }

    public SettlementItem(Guid id)
    {
        Id = id;
    }
}
