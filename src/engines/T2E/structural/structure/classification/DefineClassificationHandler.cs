using Whycespace.Domain.StructuralSystem.Structure.Classification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Engines.T2E.Structural.Structure.Classification;

public sealed class DefineClassificationHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineClassificationCommand cmd) return Task.CompletedTask;
        var aggregate = ClassificationAggregate.Define(
            new ClassificationId(cmd.ClassificationId),
            new ClassificationDescriptor(cmd.ClassificationName, cmd.ClassificationCategory));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
