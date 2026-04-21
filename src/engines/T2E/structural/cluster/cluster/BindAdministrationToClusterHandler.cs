using Whycespace.Domain.StructuralSystem.Cluster.Cluster;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Cluster;

public sealed class BindAdministrationToClusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not BindAdministrationToClusterCommand cmd) return;
        var aggregate = (ClusterAggregate)await context.LoadAggregateAsync(typeof(ClusterAggregate));
        aggregate.RecordAdministrationAttached(new ClusterAdministrationRef(cmd.AdministrationId));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
