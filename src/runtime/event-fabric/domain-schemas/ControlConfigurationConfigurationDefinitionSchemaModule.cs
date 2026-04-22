using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationDefinition;
using DomainEvents = Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlConfigurationConfigurationDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConfigurationDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationDefinedEvent), typeof(ConfigurationDefinedEventSchema));
        sink.RegisterSchema("ConfigurationDefinitionDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationDefinitionDeprecatedEvent), typeof(ConfigurationDefinitionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationDefinedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationDefinedEvent)e;
            return new ConfigurationDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.ValueType.ToString(),
                evt.Description,
                evt.DefaultValue);
        });
        sink.RegisterPayloadMapper("ConfigurationDefinitionDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationDefinitionDeprecatedEvent)e;
            return new ConfigurationDefinitionDeprecatedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
