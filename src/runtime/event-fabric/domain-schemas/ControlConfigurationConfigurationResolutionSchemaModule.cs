using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationResolution;
using DomainEvents = Whycespace.Domain.ControlSystem.Configuration.ConfigurationResolution;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlConfigurationConfigurationResolutionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConfigurationResolvedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationResolvedEvent), typeof(ConfigurationResolvedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationResolvedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationResolvedEvent)e;
            return new ConfigurationResolvedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.DefinitionId,
                evt.ScopeId,
                evt.StateId,
                evt.ResolvedValue,
                evt.ResolvedAt);
        });
    }
}
