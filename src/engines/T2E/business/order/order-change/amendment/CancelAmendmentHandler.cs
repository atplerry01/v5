using Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.Amendment;

public sealed class CancelAmendmentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CancelAmendmentCommand cmd)
            return;

        var aggregate = (AmendmentAggregate)await context.LoadAggregateAsync(typeof(AmendmentAggregate));
        aggregate.Cancel(cmd.CancelledAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
