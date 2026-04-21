using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class CloseLifecycleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CloseLifecycleCommand cmd)
            return;

        var aggregate = (LifecycleAggregate)await context.LoadAggregateAsync(typeof(LifecycleAggregate));
        aggregate.Close(cmd.ClosedAt);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
