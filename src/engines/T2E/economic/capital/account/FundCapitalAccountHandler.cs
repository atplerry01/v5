using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Account;

public sealed class FundCapitalAccountHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FundCapitalAccountCommand cmd)
            return;

        var aggregate = (CapitalAccountAggregate)await context.LoadAggregateAsync(typeof(CapitalAccountAggregate));
        aggregate.Fund(new Amount(cmd.Amount), new Currency(cmd.Currency));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
