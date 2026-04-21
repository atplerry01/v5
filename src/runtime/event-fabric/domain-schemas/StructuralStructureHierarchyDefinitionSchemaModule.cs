using Whycespace.Shared.Contracts.Events.Structural.Structure.HierarchyDefinition;
using DomainEvents = Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralStructureHierarchyDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("HierarchyDefinitionDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.HierarchyDefinitionDefinedEvent), typeof(HierarchyDefinitionDefinedEventSchema));
        sink.RegisterSchema("HierarchyDefinitionValidatedEvent", EventVersion.Default,
            typeof(DomainEvents.HierarchyDefinitionValidatedEvent), typeof(HierarchyDefinitionValidatedEventSchema));
        sink.RegisterSchema("HierarchyDefinitionLockedEvent", EventVersion.Default,
            typeof(DomainEvents.HierarchyDefinitionLockedEvent), typeof(HierarchyDefinitionLockedEventSchema));

        sink.RegisterPayloadMapper("HierarchyDefinitionDefinedEvent", e =>
        {
            var evt = (DomainEvents.HierarchyDefinitionDefinedEvent)e;
            return new HierarchyDefinitionDefinedEventSchema(evt.HierarchyDefinitionId.Value, evt.Descriptor.HierarchyName, evt.Descriptor.ParentReference);
        });
        sink.RegisterPayloadMapper("HierarchyDefinitionValidatedEvent", e =>
        {
            var evt = (DomainEvents.HierarchyDefinitionValidatedEvent)e;
            return new HierarchyDefinitionValidatedEventSchema(evt.HierarchyDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("HierarchyDefinitionLockedEvent", e =>
        {
            var evt = (DomainEvents.HierarchyDefinitionLockedEvent)e;
            return new HierarchyDefinitionLockedEventSchema(evt.HierarchyDefinitionId.Value);
        });
    }
}
