using Whycespace.Shared.Contracts.Events.Structural.Cluster.Cluster;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Cluster;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterClusterSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ClusterDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterDefinedEvent), typeof(ClusterDefinedEventSchema));
        sink.RegisterSchema("ClusterActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterActivatedEvent), typeof(ClusterActivatedEventSchema));
        sink.RegisterSchema("ClusterArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterArchivedEvent), typeof(ClusterArchivedEventSchema));
        sink.RegisterSchema("ClusterAuthorityBoundEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterAuthorityBoundEvent), typeof(ClusterAuthorityBoundEventSchema));
        sink.RegisterSchema("ClusterAuthorityReleasedEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterAuthorityReleasedEvent), typeof(ClusterAuthorityReleasedEventSchema));
        sink.RegisterSchema("ClusterAdministrationBoundEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterAdministrationBoundEvent), typeof(ClusterAdministrationBoundEventSchema));
        sink.RegisterSchema("ClusterAdministrationReleasedEvent", EventVersion.Default,
            typeof(DomainEvents.ClusterAdministrationReleasedEvent), typeof(ClusterAdministrationReleasedEventSchema));

        sink.RegisterPayloadMapper("ClusterDefinedEvent", e =>
        {
            var evt = (DomainEvents.ClusterDefinedEvent)e;
            return new ClusterDefinedEventSchema(
                evt.ClusterId.Value,
                evt.Descriptor.ClusterName,
                evt.Descriptor.ClusterType);
        });
        sink.RegisterPayloadMapper("ClusterActivatedEvent", e =>
        {
            var evt = (DomainEvents.ClusterActivatedEvent)e;
            return new ClusterActivatedEventSchema(evt.ClusterId.Value);
        });
        sink.RegisterPayloadMapper("ClusterArchivedEvent", e =>
        {
            var evt = (DomainEvents.ClusterArchivedEvent)e;
            return new ClusterArchivedEventSchema(evt.ClusterId.Value);
        });
        sink.RegisterPayloadMapper("ClusterAuthorityBoundEvent", e =>
        {
            var evt = (DomainEvents.ClusterAuthorityBoundEvent)e;
            return new ClusterAuthorityBoundEventSchema(evt.ClusterId.Value, evt.Authority.Value);
        });
        sink.RegisterPayloadMapper("ClusterAuthorityReleasedEvent", e =>
        {
            var evt = (DomainEvents.ClusterAuthorityReleasedEvent)e;
            return new ClusterAuthorityReleasedEventSchema(evt.ClusterId.Value, evt.Authority.Value);
        });
        sink.RegisterPayloadMapper("ClusterAdministrationBoundEvent", e =>
        {
            var evt = (DomainEvents.ClusterAdministrationBoundEvent)e;
            return new ClusterAdministrationBoundEventSchema(evt.ClusterId.Value, evt.Administration.Value);
        });
        sink.RegisterPayloadMapper("ClusterAdministrationReleasedEvent", e =>
        {
            var evt = (DomainEvents.ClusterAdministrationReleasedEvent)e;
            return new ClusterAdministrationReleasedEventSchema(evt.ClusterId.Value, evt.Administration.Value);
        });
    }
}
