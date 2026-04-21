using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public sealed class ActivateClusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateClusterCommand) return;
        var aggregate = (ClusterAggregate)await context.LoadAggregateAsync(typeof(ClusterAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
