using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public sealed class ActivateSpvHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateSpvCommand) return;
        var aggregate = (SpvAggregate)await context.LoadAggregateAsync(typeof(SpvAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
