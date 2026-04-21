using Whycespace.Shared.Contracts.Events.Structural.Cluster.Topology;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Topology;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterTopologySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("TopologyDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinedEvent), typeof(TopologyDefinedEventSchema));
        sink.RegisterSchema("TopologyValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyValidatedEvent), typeof(TopologyValidatedEventSchema));
        sink.RegisterSchema("TopologyLockedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyLockedEvent), typeof(TopologyLockedEventSchema));

        sink.RegisterPayloadMapper("TopologyDefinedEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinedEvent)e;
            return new TopologyDefinedEventSchema(
                evt.TopologyId.Value,
                evt.Descriptor.ClusterReference,
                evt.Descriptor.TopologyName);
        });
        sink.RegisterPayloadMapper("TopologyValidatedEvent", e =>
        {
            var evt = (DomainEvents.TopologyValidatedEvent)e;
            return new TopologyValidatedEventSchema(evt.TopologyId.Value);
        });
        sink.RegisterPayloadMapper("TopologyLockedEvent", e =>
        {
            var evt = (DomainEvents.TopologyLockedEvent)e;
            return new TopologyLockedEventSchema(evt.TopologyId.Value);
        });
    }
}
