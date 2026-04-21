using Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.HierarchyDefinition;

public sealed class LockHierarchyDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not LockHierarchyDefinitionCommand) return;
        var aggregate = (HierarchyDefinitionAggregate)await context.LoadAggregateAsync(typeof(HierarchyDefinitionAggregate));
        aggregate.Lock();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
