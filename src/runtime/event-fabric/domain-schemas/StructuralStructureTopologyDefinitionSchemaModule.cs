using Whycespace.Shared.Contracts.Events.Structural.Structure.TopologyDefinition;
using DomainEvents = Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralStructureTopologyDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("TopologyDefinitionCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinitionCreatedEvent), typeof(TopologyDefinitionCreatedEventSchema));
        sink.RegisterSchema("TopologyDefinitionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinitionActivatedEvent), typeof(TopologyDefinitionActivatedEventSchema));
        sink.RegisterSchema("TopologyDefinitionSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinitionSuspendedEvent), typeof(TopologyDefinitionSuspendedEventSchema));
        sink.RegisterSchema("TopologyDefinitionReactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinitionReactivatedEvent), typeof(TopologyDefinitionReactivatedEventSchema));
        sink.RegisterSchema("TopologyDefinitionRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.TopologyDefinitionRetiredEvent), typeof(TopologyDefinitionRetiredEventSchema));

        sink.RegisterPayloadMapper("TopologyDefinitionCreatedEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinitionCreatedEvent)e;
            return new TopologyDefinitionCreatedEventSchema(evt.TopologyDefinitionId.Value, evt.Descriptor.DefinitionName, evt.Descriptor.DefinitionKind);
        });
        sink.RegisterPayloadMapper("TopologyDefinitionActivatedEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinitionActivatedEvent)e;
            return new TopologyDefinitionActivatedEventSchema(evt.TopologyDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("TopologyDefinitionSuspendedEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinitionSuspendedEvent)e;
            return new TopologyDefinitionSuspendedEventSchema(evt.TopologyDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("TopologyDefinitionReactivatedEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinitionReactivatedEvent)e;
            return new TopologyDefinitionReactivatedEventSchema(evt.TopologyDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("TopologyDefinitionRetiredEvent", e =>
        {
            var evt = (DomainEvents.TopologyDefinitionRetiredEvent)e;
            return new TopologyDefinitionRetiredEventSchema(evt.TopologyDefinitionId.Value);
        });
    }
}
