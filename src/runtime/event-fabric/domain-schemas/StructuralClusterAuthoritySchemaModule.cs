using Whycespace.Shared.Contracts.Events.Structural.Cluster.Authority;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Authority;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterAuthoritySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuthorityEstablishedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityEstablishedEvent), typeof(AuthorityEstablishedEventSchema));
        sink.RegisterSchema("AuthorityAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityAttachedEvent), typeof(AuthorityAttachedEventSchema));
        sink.RegisterSchema("AuthorityBindingValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityBindingValidatedEvent), typeof(AuthorityBindingValidatedEventSchema));
        sink.RegisterSchema("AuthorityActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityActivatedEvent), typeof(AuthorityActivatedEventSchema));
        sink.RegisterSchema("AuthorityRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityRevokedEvent), typeof(AuthorityRevokedEventSchema));
        sink.RegisterSchema("AuthoritySuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthoritySuspendedEvent), typeof(AuthoritySuspendedEventSchema));
        sink.RegisterSchema("AuthorityReactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityReactivatedEvent), typeof(AuthorityReactivatedEventSchema));
        sink.RegisterSchema("AuthorityRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorityRetiredEvent), typeof(AuthorityRetiredEventSchema));

        sink.RegisterPayloadMapper("AuthorityEstablishedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityEstablishedEvent)e;
            return new AuthorityEstablishedEventSchema(
                evt.AuthorityId.Value,
                evt.Descriptor.ClusterReference.Value,
                evt.Descriptor.AuthorityName);
        });
        sink.RegisterPayloadMapper("AuthorityAttachedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityAttachedEvent)e;
            return new AuthorityAttachedEventSchema(
                evt.AuthorityId.Value,
                evt.ClusterRef.Value,
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("AuthorityBindingValidatedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityBindingValidatedEvent)e;
            return new AuthorityBindingValidatedEventSchema(
                evt.AuthorityId.Value,
                evt.Parent.Value,
                evt.ParentState.ToString(),
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("AuthorityActivatedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityActivatedEvent)e;
            return new AuthorityActivatedEventSchema(evt.AuthorityId.Value);
        });
        sink.RegisterPayloadMapper("AuthorityRevokedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityRevokedEvent)e;
            return new AuthorityRevokedEventSchema(evt.AuthorityId.Value);
        });
        sink.RegisterPayloadMapper("AuthoritySuspendedEvent", e =>
        {
            var evt = (DomainEvents.AuthoritySuspendedEvent)e;
            return new AuthoritySuspendedEventSchema(evt.AuthorityId.Value);
        });
        sink.RegisterPayloadMapper("AuthorityReactivatedEvent", e =>
        {
            var evt = (DomainEvents.AuthorityReactivatedEvent)e;
            return new AuthorityReactivatedEventSchema(evt.AuthorityId.Value);
        });
        sink.RegisterPayloadMapper("AuthorityRetiredEvent", e =>
        {
            var evt = (DomainEvents.AuthorityRetiredEvent)e;
            return new AuthorityRetiredEventSchema(evt.AuthorityId.Value);
        });
    }
}
