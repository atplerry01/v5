using Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

namespace Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;

public sealed class DefineLifecycleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineLifecycleCommand cmd) return Task.CompletedTask;
        var aggregate = LifecycleAggregate.Define(
            new LifecycleId(cmd.LifecycleId),
            new LifecycleDescriptor(cmd.ClusterReference, cmd.LifecycleName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
