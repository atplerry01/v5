using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Authority;

namespace Whycespace.Engines.T2E.Structural.Cluster.Authority;

/// <summary>
/// Invokes the 4-param <c>Establish</c> factory — raises
/// <c>AuthorityEstablishedEvent</c>, <c>AuthorityAttachedEvent</c>, and
/// <c>AuthorityBindingValidatedEvent</c>.
/// </summary>
public sealed class EstablishAuthorityWithParentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public EstablishAuthorityWithParentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EstablishAuthorityWithParentCommand cmd) return Task.CompletedTask;
        var aggregate = AuthorityAggregate.Establish(
            new AuthorityId(cmd.AuthorityId),
            new AuthorityDescriptor(new ClusterRef(cmd.ClusterReference), cmd.AuthorityName),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
