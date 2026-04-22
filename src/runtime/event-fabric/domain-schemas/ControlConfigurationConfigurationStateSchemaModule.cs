using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationState;
using DomainEvents = Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlConfigurationConfigurationStateSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConfigurationStateSetEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationStateSetEvent), typeof(ConfigurationStateSetEventSchema));
        sink.RegisterSchema("ConfigurationStateRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationStateRevokedEvent), typeof(ConfigurationStateRevokedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationStateSetEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationStateSetEvent)e;
            return new ConfigurationStateSetEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.DefinitionId,
                evt.Value,
                evt.Version);
        });
        sink.RegisterPayloadMapper("ConfigurationStateRevokedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationStateRevokedEvent)e;
            return new ConfigurationStateRevokedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
