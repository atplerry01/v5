using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public sealed class SuspendSpvHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendSpvCommand) return;
        var aggregate = (SpvAggregate)await context.LoadAggregateAsync(typeof(SpvAggregate));
        aggregate.Suspend();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
