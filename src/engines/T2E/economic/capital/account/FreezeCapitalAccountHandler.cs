using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Account;

public sealed class FreezeCapitalAccountHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FreezeCapitalAccountCommand cmd)
            return;

        var aggregate = (CapitalAccountAggregate)await context.LoadAggregateAsync(typeof(CapitalAccountAggregate));
        aggregate.Freeze(cmd.Reason);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
