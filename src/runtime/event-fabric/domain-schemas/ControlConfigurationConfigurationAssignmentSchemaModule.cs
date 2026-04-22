using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationAssignment;
using DomainEvents = Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlConfigurationConfigurationAssignmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConfigurationAssignedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationAssignedEvent), typeof(ConfigurationAssignedEventSchema));
        sink.RegisterSchema("ConfigurationAssignmentRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationAssignmentRevokedEvent), typeof(ConfigurationAssignmentRevokedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationAssignedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationAssignedEvent)e;
            return new ConfigurationAssignedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.DefinitionId,
                evt.ScopeId,
                evt.Value);
        });
        sink.RegisterPayloadMapper("ConfigurationAssignmentRevokedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationAssignmentRevokedEvent)e;
            return new ConfigurationAssignmentRevokedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
