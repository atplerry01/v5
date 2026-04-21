using Whycespace.Domain.StructuralSystem.Cluster.Administration;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Administration;

namespace Whycespace.Engines.T2E.Structural.Cluster.Administration;

/// <summary>
/// Invokes the 4-param <c>Establish</c> factory which validates the parent
/// cluster via <see cref="IStructuralParentLookup"/> and raises all three
/// events: <c>AdministrationEstablishedEvent</c>,
/// <c>AdministrationAttachedEvent</c>, and
/// <c>AdministrationBindingValidatedEvent</c>.
/// </summary>
public sealed class EstablishAdministrationWithParentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public EstablishAdministrationWithParentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not EstablishAdministrationWithParentCommand cmd) return Task.CompletedTask;
        var aggregate = AdministrationAggregate.Establish(
            new AdministrationId(cmd.AdministrationId),
            new AdministrationDescriptor(new ClusterRef(cmd.ClusterReference), cmd.AdministrationName),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
