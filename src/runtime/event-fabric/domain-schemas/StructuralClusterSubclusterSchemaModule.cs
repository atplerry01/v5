using Whycespace.Shared.Contracts.Events.Structural.Cluster.Subcluster;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterSubclusterSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SubclusterDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterDefinedEvent), typeof(SubclusterDefinedEventSchema));
        sink.RegisterSchema("SubclusterAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterAttachedEvent), typeof(SubclusterAttachedEventSchema));
        sink.RegisterSchema("SubclusterBindingValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterBindingValidatedEvent), typeof(SubclusterBindingValidatedEventSchema));
        sink.RegisterSchema("SubclusterActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterActivatedEvent), typeof(SubclusterActivatedEventSchema));
        sink.RegisterSchema("SubclusterSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterSuspendedEvent), typeof(SubclusterSuspendedEventSchema));
        sink.RegisterSchema("SubclusterReactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterReactivatedEvent), typeof(SubclusterReactivatedEventSchema));
        sink.RegisterSchema("SubclusterArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterArchivedEvent), typeof(SubclusterArchivedEventSchema));
        sink.RegisterSchema("SubclusterRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.SubclusterRetiredEvent), typeof(SubclusterRetiredEventSchema));

        sink.RegisterPayloadMapper("SubclusterDefinedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterDefinedEvent)e;
            return new SubclusterDefinedEventSchema(
                evt.SubclusterId.Value,
                evt.Descriptor.ParentClusterReference.Value,
                evt.Descriptor.SubclusterName);
        });
        sink.RegisterPayloadMapper("SubclusterAttachedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterAttachedEvent)e;
            return new SubclusterAttachedEventSchema(
                evt.SubclusterId.Value,
                evt.ClusterRef.Value,
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("SubclusterBindingValidatedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterBindingValidatedEvent)e;
            return new SubclusterBindingValidatedEventSchema(
                evt.SubclusterId.Value,
                evt.Parent.Value,
                evt.ParentState.ToString(),
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("SubclusterActivatedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterActivatedEvent)e;
            return new SubclusterActivatedEventSchema(evt.SubclusterId.Value);
        });
        sink.RegisterPayloadMapper("SubclusterSuspendedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterSuspendedEvent)e;
            return new SubclusterSuspendedEventSchema(evt.SubclusterId.Value);
        });
        sink.RegisterPayloadMapper("SubclusterReactivatedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterReactivatedEvent)e;
            return new SubclusterReactivatedEventSchema(evt.SubclusterId.Value);
        });
        sink.RegisterPayloadMapper("SubclusterArchivedEvent", e =>
        {
            var evt = (DomainEvents.SubclusterArchivedEvent)e;
            return new SubclusterArchivedEventSchema(evt.SubclusterId.Value);
        });
        sink.RegisterPayloadMapper("SubclusterRetiredEvent", e =>
        {
            var evt = (DomainEvents.SubclusterRetiredEvent)e;
            return new SubclusterRetiredEventSchema(evt.SubclusterId.Value);
        });
    }
}
