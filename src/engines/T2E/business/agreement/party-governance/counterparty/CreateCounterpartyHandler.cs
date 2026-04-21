using Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.PartyGovernance.Counterparty;

public sealed class CreateCounterpartyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCounterpartyCommand cmd)
            return Task.CompletedTask;

        var profile = new CounterpartyProfile(cmd.IdentityReference, cmd.Name);
        var aggregate = CounterpartyAggregate.Create(new CounterpartyId(cmd.CounterpartyId), profile);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
