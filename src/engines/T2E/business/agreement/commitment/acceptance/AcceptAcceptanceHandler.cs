using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Acceptance;

public sealed class AcceptAcceptanceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AcceptAcceptanceCommand)
            return;

        var aggregate = (AcceptanceAggregate)await context.LoadAggregateAsync(typeof(AcceptanceAggregate));
        aggregate.Accept();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
