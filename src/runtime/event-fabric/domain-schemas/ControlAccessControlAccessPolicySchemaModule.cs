using Whycespace.Shared.Contracts.Events.Control.AccessControl.AccessPolicy;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.AccessPolicy;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlAccessPolicySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AccessPolicyDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.AccessPolicyDefinedEvent), typeof(AccessPolicyDefinedEventSchema));
        sink.RegisterSchema("AccessPolicyActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.AccessPolicyActivatedEvent), typeof(AccessPolicyActivatedEventSchema));
        sink.RegisterSchema("AccessPolicyRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.AccessPolicyRetiredEvent), typeof(AccessPolicyRetiredEventSchema));

        sink.RegisterPayloadMapper("AccessPolicyDefinedEvent", e =>
        {
            var evt = (DomainEvents.AccessPolicyDefinedEvent)e;
            return new AccessPolicyDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Scope,
                evt.AllowedRoleIds.ToList());
        });
        sink.RegisterPayloadMapper("AccessPolicyActivatedEvent", e =>
        {
            var evt = (DomainEvents.AccessPolicyActivatedEvent)e;
            return new AccessPolicyActivatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
        sink.RegisterPayloadMapper("AccessPolicyRetiredEvent", e =>
        {
            var evt = (DomainEvents.AccessPolicyRetiredEvent)e;
            return new AccessPolicyRetiredEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
