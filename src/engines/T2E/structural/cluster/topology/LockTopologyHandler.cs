using Whycespace.Domain.StructuralSystem.Cluster.Topology;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Engines.T2E.Structural.Cluster.Topology;

public sealed class LockTopologyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not LockTopologyCommand) return;
        var aggregate = (TopologyAggregate)await context.LoadAggregateAsync(typeof(TopologyAggregate));
        aggregate.Lock();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
