using Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.TypeDefinition;

public sealed class DefineTypeDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DefineTypeDefinitionCommand cmd) return Task.CompletedTask;
        var aggregate = TypeDefinitionAggregate.Define(
            new TypeDefinitionId(cmd.TypeDefinitionId),
            new TypeDefinitionDescriptor(cmd.TypeName, cmd.TypeCategory));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
