using Whycespace.Domain.StructuralSystem.Cluster.Provider;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Engines.T2E.Structural.Cluster.Provider;

public sealed class ReactivateProviderHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ReactivateProviderCommand) return;
        var aggregate = (ProviderAggregate)await context.LoadAggregateAsync(typeof(ProviderAggregate));
        aggregate.Reactivate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
