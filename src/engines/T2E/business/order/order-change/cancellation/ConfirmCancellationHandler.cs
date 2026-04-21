using Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Order.OrderChange.Cancellation;

public sealed class ConfirmCancellationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ConfirmCancellationCommand cmd)
            return;

        var aggregate = (CancellationAggregate)await context.LoadAggregateAsync(typeof(CancellationAggregate));
        aggregate.Confirm(cmd.ConfirmedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
