using Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyDefinition;
using DomainEvents = Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlSystemPolicyPolicyDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PolicyDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyDefinedEvent), typeof(PolicyDefinedEventSchema));
        sink.RegisterSchema("PolicyDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.PolicyDeprecatedEvent), typeof(PolicyDeprecatedEventSchema));

        sink.RegisterPayloadMapper("PolicyDefinedEvent", e =>
        {
            var evt = (DomainEvents.PolicyDefinedEvent)e;
            return new PolicyDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Scope.Classification,
                evt.Scope.Context,
                evt.Scope.ActionMask,
                evt.Version);
        });
        sink.RegisterPayloadMapper("PolicyDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.PolicyDeprecatedEvent)e;
            return new PolicyDeprecatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
