using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

/// <summary>
/// Invokes the 4-param <c>Create</c> factory — raises
/// <c>SpvCreatedEvent</c>, <c>SpvAttachedEvent</c>, and
/// <c>SpvBindingValidatedEvent</c>.
/// </summary>
public sealed class CreateSpvWithParentHandler : IEngine
{
    private readonly IStructuralParentLookup _parentLookup;

    public CreateSpvWithParentHandler(IStructuralParentLookup parentLookup)
    {
        _parentLookup = parentLookup;
    }

    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSpvWithParentCommand cmd) return Task.CompletedTask;
        var aggregate = SpvAggregate.Create(
            new SpvId(cmd.SpvId),
            new SpvDescriptor(new ClusterRef(cmd.ClusterReference), cmd.SpvName, Enum.Parse<SpvType>(cmd.SpvType)),
            cmd.EffectiveAt,
            _parentLookup);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
