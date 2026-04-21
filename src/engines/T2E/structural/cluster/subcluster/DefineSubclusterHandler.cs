using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class DefineSubclusterHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSubclusterCommand cmd) return Task.CompletedTask;
        var aggregate = SubclusterAggregate.Define(
            new SubclusterId(cmd.SubclusterId),
            new SubclusterDescriptor(new ClusterRef(cmd.ParentClusterReference), cmd.SubclusterName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
