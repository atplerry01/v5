using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Agreement.Commitment.Obligation;

public sealed class FulfillObligationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FulfillObligationCommand)
            return;

        var aggregate = (ObligationAggregate)await context.LoadAggregateAsync(typeof(ObligationAggregate));
        aggregate.Fulfill();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
