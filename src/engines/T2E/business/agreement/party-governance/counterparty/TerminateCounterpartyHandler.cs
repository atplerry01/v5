using Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Counterparty;

public sealed class TerminateCounterpartyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TerminateCounterpartyCommand)
            return;

        var aggregate = (CounterpartyAggregate)await context.LoadAggregateAsync(typeof(CounterpartyAggregate));
        aggregate.Terminate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
