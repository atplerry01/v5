using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Acceptance;

public sealed class RejectAcceptanceHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RejectAcceptanceCommand)
            return;

        var aggregate = (AcceptanceAggregate)await context.LoadAggregateAsync(typeof(AcceptanceAggregate));
        aggregate.Reject();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
