using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

namespace Whycespace.Engines.T2E.Platform.Schema.SchemaDefinition;

public sealed class DeprecateSchemaDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateSchemaDefinitionCommand cmd)
            return;

        var aggregate = (SchemaDefinitionAggregate)await context.LoadAggregateAsync(typeof(SchemaDefinitionAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
