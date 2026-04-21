using Whycespace.Domain.StructuralSystem.Cluster.Topology;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Engines.T2E.Structural.Cluster.Topology;

public sealed class DefineTopologyHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineTopologyCommand cmd) return Task.CompletedTask;
        var aggregate = TopologyAggregate.Define(
            new TopologyId(cmd.TopologyId),
            new TopologyDescriptor(cmd.ClusterReference, cmd.TopologyName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
