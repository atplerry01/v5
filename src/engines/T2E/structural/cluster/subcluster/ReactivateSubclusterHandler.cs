using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class ReactivateSubclusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReactivateSubclusterCommand) return;
        var aggregate = (SubclusterAggregate)await context.LoadAggregateAsync(typeof(SubclusterAggregate));
        aggregate.Reactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
