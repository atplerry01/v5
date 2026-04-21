using Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

namespace Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;

public sealed class TransitionLifecycleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not TransitionLifecycleCommand) return;
        var aggregate = (LifecycleAggregate)await context.LoadAggregateAsync(typeof(LifecycleAggregate));
        aggregate.Transition();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
