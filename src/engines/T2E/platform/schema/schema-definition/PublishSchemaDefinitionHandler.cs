using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;

namespace Whycespace.Engines.T2E.Platform.Schema.SchemaDefinition;

public sealed class PublishSchemaDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PublishSchemaDefinitionCommand cmd)
            return;

        var aggregate = (SchemaDefinitionAggregate)await context.LoadAggregateAsync(typeof(SchemaDefinitionAggregate));
        aggregate.Publish(new Timestamp(cmd.PublishedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
