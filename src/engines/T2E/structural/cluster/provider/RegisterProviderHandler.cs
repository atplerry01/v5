using Whycespace.Domain.StructuralSystem.Cluster.Provider;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Engines.T2E.Structural.Cluster.Provider;

public sealed class RegisterProviderHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterProviderCommand cmd) return Task.CompletedTask;
        var aggregate = ProviderAggregate.Register(
            new ProviderId(cmd.ProviderId),
            new ProviderProfile(new ClusterRef(cmd.ClusterReference), cmd.ProviderName));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
