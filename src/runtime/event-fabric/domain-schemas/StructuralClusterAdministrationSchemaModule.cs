using Whycespace.Shared.Contracts.Events.Structural.Cluster.Administration;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Administration;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterAdministrationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AdministrationEstablishedEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationEstablishedEvent), typeof(AdministrationEstablishedEventSchema));
        sink.RegisterSchema("AdministrationAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationAttachedEvent), typeof(AdministrationAttachedEventSchema));
        sink.RegisterSchema("AdministrationBindingValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationBindingValidatedEvent), typeof(AdministrationBindingValidatedEventSchema));
        sink.RegisterSchema("AdministrationActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationActivatedEvent), typeof(AdministrationActivatedEventSchema));
        sink.RegisterSchema("AdministrationSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationSuspendedEvent), typeof(AdministrationSuspendedEventSchema));
        sink.RegisterSchema("AdministrationRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.AdministrationRetiredEvent), typeof(AdministrationRetiredEventSchema));

        sink.RegisterPayloadMapper("AdministrationEstablishedEvent", e =>
        {
            var evt = (DomainEvents.AdministrationEstablishedEvent)e;
            return new AdministrationEstablishedEventSchema(
                evt.AdministrationId.Value,
                evt.Descriptor.ClusterReference.Value,
                evt.Descriptor.AdministrationName);
        });
        sink.RegisterPayloadMapper("AdministrationAttachedEvent", e =>
        {
            var evt = (DomainEvents.AdministrationAttachedEvent)e;
            return new AdministrationAttachedEventSchema(
                evt.AdministrationId.Value,
                evt.ClusterRef.Value,
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("AdministrationBindingValidatedEvent", e =>
        {
            var evt = (DomainEvents.AdministrationBindingValidatedEvent)e;
            return new AdministrationBindingValidatedEventSchema(
                evt.AdministrationId.Value,
                evt.Parent.Value,
                evt.ParentState.ToString(),
                evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("AdministrationActivatedEvent", e =>
        {
            var evt = (DomainEvents.AdministrationActivatedEvent)e;
            return new AdministrationActivatedEventSchema(evt.AdministrationId.Value);
        });
        sink.RegisterPayloadMapper("AdministrationSuspendedEvent", e =>
        {
            var evt = (DomainEvents.AdministrationSuspendedEvent)e;
            return new AdministrationSuspendedEventSchema(evt.AdministrationId.Value);
        });
        sink.RegisterPayloadMapper("AdministrationRetiredEvent", e =>
        {
            var evt = (DomainEvents.AdministrationRetiredEvent)e;
            return new AdministrationRetiredEventSchema(evt.AdministrationId.Value);
        });
    }
}
