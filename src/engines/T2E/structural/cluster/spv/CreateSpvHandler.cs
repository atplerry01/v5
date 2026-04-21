using Whycespace.Domain.StructuralSystem.Cluster.Spv;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Engines.T2E.Structural.Cluster.Spv;

public sealed class CreateSpvHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateSpvCommand cmd) return Task.CompletedTask;
        if (!Enum.TryParse<SpvType>(cmd.SpvType, ignoreCase: false, out var parsedType))
            throw new InvalidOperationException($"SpvType '{cmd.SpvType}' is not a defined value.");
        var aggregate = SpvAggregate.Create(
            new SpvId(cmd.SpvId),
            new SpvDescriptor(new ClusterRef(cmd.ClusterReference), cmd.SpvName, parsedType));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
