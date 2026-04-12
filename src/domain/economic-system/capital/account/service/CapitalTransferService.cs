using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed class CapitalTransferService
{
    public void Transfer(
        CapitalAccountAggregate source,
        CapitalAccountAggregate destination,
        Amount amount,
        Currency currency)
    {
        if (source.Currency != destination.Currency)
            throw CapitalAccountErrors.CurrencyMismatch(source.Currency, destination.Currency);

        source.Allocate(amount, currency);
        destination.Fund(amount, currency);
    }
}
