using Whycespace.Shared.Contracts.Events.Structural.Cluster.Spv;
using DomainEvents = Whycespace.Domain.StructuralSystem.Cluster.Spv;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralClusterSpvSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SpvCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvCreatedEvent), typeof(SpvCreatedEventSchema));
        sink.RegisterSchema("SpvAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvAttachedEvent), typeof(SpvAttachedEventSchema));
        sink.RegisterSchema("SpvBindingValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvBindingValidatedEvent), typeof(SpvBindingValidatedEventSchema));
        sink.RegisterSchema("SpvActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvActivatedEvent), typeof(SpvActivatedEventSchema));
        sink.RegisterSchema("SpvSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvSuspendedEvent), typeof(SpvSuspendedEventSchema));
        sink.RegisterSchema("SpvClosedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvClosedEvent), typeof(SpvClosedEventSchema));
        sink.RegisterSchema("SpvReactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.SpvReactivatedEvent), typeof(SpvReactivatedEventSchema));
        sink.RegisterSchema("SpvRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.SpvRetiredEvent), typeof(SpvRetiredEventSchema));

        sink.RegisterPayloadMapper("SpvCreatedEvent", e =>
        {
            var evt = (DomainEvents.SpvCreatedEvent)e;
            return new SpvCreatedEventSchema(evt.SpvId.Value, evt.Descriptor.ClusterReference.Value, evt.Descriptor.SpvName, evt.Descriptor.SpvType.ToString());
        });
        sink.RegisterPayloadMapper("SpvAttachedEvent", e =>
        {
            var evt = (DomainEvents.SpvAttachedEvent)e;
            return new SpvAttachedEventSchema(evt.SpvId.Value, evt.ClusterRef.Value, evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("SpvBindingValidatedEvent", e =>
        {
            var evt = (DomainEvents.SpvBindingValidatedEvent)e;
            return new SpvBindingValidatedEventSchema(evt.SpvId.Value, evt.Parent.Value, evt.ParentState.ToString(), evt.EffectiveAt);
        });
        sink.RegisterPayloadMapper("SpvActivatedEvent", e =>
        {
            var evt = (DomainEvents.SpvActivatedEvent)e;
            return new SpvActivatedEventSchema(evt.SpvId.Value);
        });
        sink.RegisterPayloadMapper("SpvSuspendedEvent", e =>
        {
            var evt = (DomainEvents.SpvSuspendedEvent)e;
            return new SpvSuspendedEventSchema(evt.SpvId.Value);
        });
        sink.RegisterPayloadMapper("SpvClosedEvent", e =>
        {
            var evt = (DomainEvents.SpvClosedEvent)e;
            return new SpvClosedEventSchema(evt.SpvId.Value);
        });
        sink.RegisterPayloadMapper("SpvReactivatedEvent", e =>
        {
            var evt = (DomainEvents.SpvReactivatedEvent)e;
            return new SpvReactivatedEventSchema(evt.SpvId.Value);
        });
        sink.RegisterPayloadMapper("SpvRetiredEvent", e =>
        {
            var evt = (DomainEvents.SpvRetiredEvent)e;
            return new SpvRetiredEventSchema(evt.SpvId.Value);
        });
    }
}
