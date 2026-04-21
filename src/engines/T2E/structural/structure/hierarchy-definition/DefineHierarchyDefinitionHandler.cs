using Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.HierarchyDefinition;

public sealed class DefineHierarchyDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineHierarchyDefinitionCommand cmd) return Task.CompletedTask;
        var aggregate = HierarchyDefinitionAggregate.Define(
            new HierarchyDefinitionId(cmd.HierarchyDefinitionId),
            new HierarchyDefinitionDescriptor(cmd.HierarchyName, cmd.ParentReference));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
