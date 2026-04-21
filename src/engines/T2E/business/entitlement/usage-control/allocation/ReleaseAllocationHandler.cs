using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Allocation;

public sealed class ReleaseAllocationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseAllocationCommand)
            return;

        var aggregate = (AllocationAggregate)await context.LoadAggregateAsync(typeof(AllocationAggregate));
        aggregate.Release();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
