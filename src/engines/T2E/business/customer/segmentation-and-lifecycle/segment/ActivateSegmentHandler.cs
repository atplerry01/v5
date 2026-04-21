using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.Segment;

public sealed class ActivateSegmentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateSegmentCommand)
            return;

        var aggregate = (SegmentAggregate)await context.LoadAggregateAsync(typeof(SegmentAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
