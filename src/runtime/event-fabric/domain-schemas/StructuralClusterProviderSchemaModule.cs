using Whycespace.Shared.Contracts.Events.Structural.Cluster.Provider;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Provider;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterProviderSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ProviderRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderRegisteredEvent), typeof(ProviderRegisteredEventSchema));
        sink.RegisterSchema("ProviderAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderAttachedEvent), typeof(ProviderAttachedEventSchema));
        sink.RegisterSchema("ProviderBindingValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderBindingValidatedEvent), typeof(ProviderBindingValidatedEventSchema));
        sink.RegisterSchema("ProviderActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderActivatedEvent), typeof(ProviderActivatedEventSchema));
        sink.RegisterSchema("ProviderSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderSuspendedEvent), typeof(ProviderSuspendedEventSchema));
        sink.RegisterSchema("ProviderReactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderReactivatedEvent), typeof(ProviderReactivatedEventSchema));
        sink.RegisterSchema("ProviderRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.ProviderRetiredEvent), typeof(ProviderRetiredEventSchema));

        sink.RegisterPayloadMapper("ProviderRegisteredEvent", e =>
        {
            var evt = (DomainEvents.ProviderRegisteredEvent)e;
            return new ProviderRegisteredEventSchema(evt.ProviderId.Value, evt.Profile.ClusterReference.Value, evt.Profile.ProviderName);
        });
        sink.RegisterPayloadMapper("ProviderAttachedEvent", e =>
        {
            var evt = (DomainEvents.ProviderAttachedEvent)e;
            return new ProviderAttachedEventSchema(evt.ProviderId.Value, evt.ClusterRef.Value, evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("ProviderBindingValidatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderBindingValidatedEvent)e;
            return new ProviderBindingValidatedEventSchema(evt.ProviderId.Value, evt.Parent.Value, evt.ParentState.ToString(), evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("ProviderActivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderActivatedEvent)e;
            return new ProviderActivatedEventSchema(evt.ProviderId.Value);
        });
        sink.RegisterPayloadMapper("ProviderSuspendedEvent", e =>
        {
            var evt = (DomainEvents.ProviderSuspendedEvent)e;
            return new ProviderSuspendedEventSchema(evt.ProviderId.Value);
        });
        sink.RegisterPayloadMapper("ProviderReactivatedEvent", e =>
        {
            var evt = (DomainEvents.ProviderReactivatedEvent)e;
            return new ProviderReactivatedEventSchema(evt.ProviderId.Value);
        });
        sink.RegisterPayloadMapper("ProviderRetiredEvent", e =>
        {
            var evt = (DomainEvents.ProviderRetiredEvent)e;
            return new ProviderRetiredEventSchema(evt.ProviderId.Value);
        });
    }
}
