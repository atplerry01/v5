using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

public sealed class ActivateSubclusterHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateSubclusterCommand) return;
        var aggregate = (SubclusterAggregate)await context.LoadAggregateAsync(typeof(SubclusterAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
