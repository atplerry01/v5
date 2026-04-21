using Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.ContactPoint;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Customer.SegmentationAndLifecycle.ContactPoint;

public sealed class UpdateContactPointHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateContactPointCommand cmd)
            return;

        var aggregate = (ContactPointAggregate)await context.LoadAggregateAsync(typeof(ContactPointAggregate));
        aggregate.Update(new ContactPointValue(cmd.Value));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
