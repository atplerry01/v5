using Whycespace.Domain.StructuralSystem.Cluster.Provider;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Provider;

namespace Whycespace.Engines.T2E.Structural.Cluster.Provider;

/// <summary>
/// Invokes the 4-param <c>Register</c> factory — raises
/// <c>ProviderRegisteredEvent</c>, <c>ProviderAttachedEvent</c>, and
/// <c>ProviderBindingValidatedEvent</c>.
/// </summary>
public sealed class RegisterProviderWithParentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public RegisterProviderWithParentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterProviderWithParentCommand cmd) return Task.CompletedTask;
        var aggregate = ProviderAggregate.Register(
            new ProviderId(cmd.ProviderId),
            new ProviderProfile(new ClusterRef(cmd.ClusterReference), cmd.ProviderName),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
