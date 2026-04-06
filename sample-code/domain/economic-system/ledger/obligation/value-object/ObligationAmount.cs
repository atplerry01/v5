namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationAmount(decimal Value)
{
    public bool IsPositive => Value > 0;
}
