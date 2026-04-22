using Whycespace.Shared.Contracts.Events.Control.Configuration.ConfigurationScope;
using DomainEvents = Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlConfigurationConfigurationScopeSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConfigurationScopeDeclaredEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationScopeDeclaredEvent), typeof(ConfigurationScopeDeclaredEventSchema));
        sink.RegisterSchema("ConfigurationScopeRemovedEvent", EventVersion.Default,
            typeof(DomainEvents.ConfigurationScopeRemovedEvent), typeof(ConfigurationScopeRemovedEventSchema));

        sink.RegisterPayloadMapper("ConfigurationScopeDeclaredEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationScopeDeclaredEvent)e;
            return new ConfigurationScopeDeclaredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.DefinitionId,
                evt.Classification,
                evt.Context);
        });
        sink.RegisterPayloadMapper("ConfigurationScopeRemovedEvent", e =>
        {
            var evt = (DomainEvents.ConfigurationScopeRemovedEvent)e;
            return new ConfigurationScopeRemovedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
