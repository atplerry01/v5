using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class ArchiveSubclusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveSubclusterCommand) return;
        var aggregate = (SubclusterAggregate)await context.LoadAggregateAsync(typeof(SubclusterAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
