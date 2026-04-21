using Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Engines.T2E.Structural.Structure.TypeDefinition;

public sealed class RetireTypeDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RetireTypeDefinitionCommand) return;
        var aggregate = (TypeDefinitionAggregate)await context.LoadAggregateAsync(typeof(TypeDefinitionAggregate));
        aggregate.Retire();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
