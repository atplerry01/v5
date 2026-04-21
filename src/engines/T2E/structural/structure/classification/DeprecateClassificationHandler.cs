using Whycespace.Domain.StructuralSystem.Structure.Classification;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.Classification;

namespace Whycespace.Engines.T2E.Structural.Structure.Classification;

public sealed class DeprecateClassificationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateClassificationCommand) return;
        var aggregate = (ClassificationAggregate)await context.LoadAggregateAsync(typeof(ClassificationAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
