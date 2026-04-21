using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public sealed class ArchiveClusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveClusterCommand) return;
        var aggregate = (ClusterAggregate)await context.LoadAggregateAsync(typeof(ClusterAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
