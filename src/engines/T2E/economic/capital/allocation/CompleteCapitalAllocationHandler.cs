using Whycespace.Domain.EconomicSystem.Capital.Allocation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Allocation;

public sealed class CompleteCapitalAllocationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CompleteCapitalAllocationCommand cmd)
            return;

        var aggregate = (CapitalAllocationAggregate)await context.LoadAggregateAsync(typeof(CapitalAllocationAggregate));
        aggregate.Complete(new Timestamp(cmd.CompletedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
