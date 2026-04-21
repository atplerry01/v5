using Whycespace.Domain.StructuralSystem.Structure.Classification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Engines.T2E.Structural.Structure.Classification;

public sealed class ActivateClassificationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateClassificationCommand) return;
        var aggregate = (ClassificationAggregate)await context.LoadAggregateAsync(typeof(ClassificationAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
