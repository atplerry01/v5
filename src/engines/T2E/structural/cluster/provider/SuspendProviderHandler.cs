using Whycespace.Domain.StructuralSystem.Cluster.Provider;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Engines.T2E.Structural.Cluster.Provider;

public sealed class SuspendProviderHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SuspendProviderCommand) return;
        var aggregate = (ProviderAggregate)await context.LoadAggregateAsync(typeof(ProviderAggregate));
        aggregate.Suspend();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
