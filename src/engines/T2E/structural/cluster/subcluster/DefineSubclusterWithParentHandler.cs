using Whycespace.Domain.StructuralSystem.Cluster.Subcluster;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Engines.T2E.Structural.Cluster.Subcluster;

/// <summary>
/// Invokes the 4-param <c>Define</c> factory — raises
/// <c>SubclusterDefinedEvent</c>, <c>SubclusterAttachedEvent</c>, and
/// <c>SubclusterBindingValidatedEvent</c>.
/// </summary>
public sealed class DefineSubclusterWithParentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public DefineSubclusterWithParentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineSubclusterWithParentCommand cmd) return Task.CompletedTask;
        var aggregate = SubclusterAggregate.Define(
            new SubclusterId(cmd.SubclusterId),
            new SubclusterDescriptor(new ClusterRef(cmd.ParentClusterReference), cmd.SubclusterName),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
