namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CapitalTransferService
{
    public void Transfer(
        CapitalAccountAggregate source,
        CapitalAccountAggregate destination,
        decimal amount,
        string currencyCode,
        DateTimeOffset now)
    {
        source.Allocate(amount, currencyCode, now);
        destination.Fund(amount, currencyCode, now);
    }
}
