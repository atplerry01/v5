using Whycespace.Shared.Contracts.Events.Platform.Command.CommandDefinition;
using DomainEvents = Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformCommandDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("CommandDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.CommandDefinedEvent), typeof(CommandDefinedEventSchema));
        sink.RegisterSchema("CommandDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.CommandDeprecatedEvent), typeof(CommandDeprecatedEventSchema));

        sink.RegisterPayloadMapper("CommandDefinedEvent", e =>
        {
            var evt = (DomainEvents.CommandDefinedEvent)e;
            return new CommandDefinedEventSchema(evt.CommandDefinitionId.Value,
                evt.TypeName.Value, evt.Version.Value.ToString(), evt.SchemaId,
                evt.OwnerRoute.Classification, evt.OwnerRoute.Context, evt.OwnerRoute.Domain);
        });
        sink.RegisterPayloadMapper("CommandDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.CommandDeprecatedEvent)e;
            return new CommandDeprecatedEventSchema(evt.CommandDefinitionId.Value);
        });
    }
}
