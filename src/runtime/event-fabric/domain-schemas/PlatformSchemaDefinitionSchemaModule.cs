using Whycespace.Shared.Contracts.Events.Platform.Schema.SchemaDefinition;
using DomainEvents = Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformSchemaDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SchemaDefinitionDraftedEvent", EventVersion.Default,
            typeof(DomainEvents.SchemaDefinitionDraftedEvent), typeof(SchemaDefinitionDraftedEventSchema));
        sink.RegisterSchema("SchemaDefinitionPublishedEvent", EventVersion.Default,
            typeof(DomainEvents.SchemaDefinitionPublishedEvent), typeof(SchemaDefinitionPublishedEventSchema));
        sink.RegisterSchema("SchemaDefinitionDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.SchemaDefinitionDeprecatedEvent), typeof(SchemaDefinitionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("SchemaDefinitionDraftedEvent", e =>
        {
            var evt = (DomainEvents.SchemaDefinitionDraftedEvent)e;
            return new SchemaDefinitionDraftedEventSchema(
                evt.SchemaDefinitionId.Value,
                evt.SchemaName.Value,
                evt.Version,
                evt.CompatibilityMode.Value);
        });
        sink.RegisterPayloadMapper("SchemaDefinitionPublishedEvent", e =>
        {
            var evt = (DomainEvents.SchemaDefinitionPublishedEvent)e;
            return new SchemaDefinitionPublishedEventSchema(evt.SchemaDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("SchemaDefinitionDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.SchemaDefinitionDeprecatedEvent)e;
            return new SchemaDefinitionDeprecatedEventSchema(evt.SchemaDefinitionId.Value);
        });
    }
}
