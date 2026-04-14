using Whycespace.Domain.EconomicSystem.Capital.Allocation;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Economic.Capital.Allocation;

public sealed class AllocateCapitalToSpvHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AllocateCapitalToSpvCommand cmd)
            return;

        var aggregate = (CapitalAllocationAggregate)await context.LoadAggregateAsync(typeof(CapitalAllocationAggregate));
        aggregate.AllocateToSpv(cmd.SpvTargetId, cmd.OwnershipPercentage);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
