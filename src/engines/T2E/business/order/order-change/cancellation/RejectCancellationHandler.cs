using Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.Cancellation;

public sealed class RejectCancellationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RejectCancellationCommand cmd)
            return;

        var aggregate = (CancellationAggregate)await context.LoadAggregateAsync(typeof(CancellationAggregate));
        aggregate.Reject(cmd.RejectedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
