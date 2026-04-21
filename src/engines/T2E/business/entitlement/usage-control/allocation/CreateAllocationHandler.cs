using Whycespace.Domain.BusinessSystem.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Entitlement.UsageControl.Allocation;

public sealed class CreateAllocationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateAllocationCommand cmd)
            return Task.CompletedTask;

        var aggregate = AllocationAggregate.Create(
            new AllocationId(cmd.AllocationId),
            new ResourceId(cmd.ResourceId),
            cmd.RequestedCapacity);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
