using Whycespace.Domain.EconomicSystem.Capital.Allocation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Allocation;

public sealed class ReleaseCapitalAllocationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReleaseCapitalAllocationCommand cmd)
            return;

        var aggregate = (CapitalAllocationAggregate)await context.LoadAggregateAsync(typeof(CapitalAllocationAggregate));
        aggregate.Release(new Timestamp(cmd.ReleasedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
