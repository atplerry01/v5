using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Account;

public sealed class CloseCapitalAccountHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CloseCapitalAccountCommand cmd)
            return;

        var aggregate = (CapitalAccountAggregate)await context.LoadAggregateAsync(typeof(CapitalAccountAggregate));
        aggregate.Close(new Timestamp(cmd.ClosedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
