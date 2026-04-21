using Whycespace.Domain.StructuralSystem.Cluster.Topology;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Engines.T2E.Structural.Cluster.Topology;

public sealed class ValidateTopologyHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ValidateTopologyCommand) return;
        var aggregate = (TopologyAggregate)await context.LoadAggregateAsync(typeof(TopologyAggregate));
        aggregate.Validate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
