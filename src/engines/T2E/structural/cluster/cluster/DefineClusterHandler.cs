using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public sealed class DefineClusterHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineClusterCommand cmd) return Task.CompletedTask;
        var aggregate = ClusterAggregate.Define(
            new ClusterId(cmd.ClusterId),
            new ClusterDescriptor(cmd.ClusterName, cmd.ClusterType));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
