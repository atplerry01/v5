using Whycespace.Shared.Contracts.Events.Structural.Cluster.Lifecycle;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Lifecycle;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterLifecycleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("LifecycleDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.LifecycleDefinedEvent), typeof(LifecycleDefinedEventSchema));
        sink.RegisterSchema("LifecycleTransitionedEvent", EventVersion.Default,
            typeof(DomainEvents.LifecycleTransitionedEvent), typeof(LifecycleTransitionedEventSchema));
        sink.RegisterSchema("LifecycleCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.LifecycleCompletedEvent), typeof(LifecycleCompletedEventSchema));

        sink.RegisterPayloadMapper("LifecycleDefinedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleDefinedEvent)e;
            return new LifecycleDefinedEventSchema(
                evt.LifecycleId.Value,
                evt.Descriptor.ClusterReference,
                evt.Descriptor.LifecycleName);
        });
        sink.RegisterPayloadMapper("LifecycleTransitionedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleTransitionedEvent)e;
            return new LifecycleTransitionedEventSchema(evt.LifecycleId.Value);
        });
        sink.RegisterPayloadMapper("LifecycleCompletedEvent", e =>
        {
            var evt = (DomainEvents.LifecycleCompletedEvent)e;
            return new LifecycleCompletedEventSchema(evt.LifecycleId.Value);
        });
    }
}
