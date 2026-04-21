using Whycespace.Shared.Contracts.Events.Structural.Structure.TypeDefinition;
using DomainEvents = Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralStructureTypeDefinitionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("TypeDefinitionDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.TypeDefinitionDefinedEvent), typeof(TypeDefinitionDefinedEventSchema));
        sink.RegisterSchema("TypeDefinitionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.TypeDefinitionActivatedEvent), typeof(TypeDefinitionActivatedEventSchema));
        sink.RegisterSchema("TypeDefinitionRetiredEvent", EventVersion.Default,
            typeof(DomainEvents.TypeDefinitionRetiredEvent), typeof(TypeDefinitionRetiredEventSchema));

        sink.RegisterPayloadMapper("TypeDefinitionDefinedEvent", e =>
        {
            var evt = (DomainEvents.TypeDefinitionDefinedEvent)e;
            return new TypeDefinitionDefinedEventSchema(evt.TypeDefinitionId.Value, evt.Descriptor.TypeName, evt.Descriptor.TypeCategory);
        });
        sink.RegisterPayloadMapper("TypeDefinitionActivatedEvent", e =>
        {
            var evt = (DomainEvents.TypeDefinitionActivatedEvent)e;
            return new TypeDefinitionActivatedEventSchema(evt.TypeDefinitionId.Value);
        });
        sink.RegisterPayloadMapper("TypeDefinitionRetiredEvent", e =>
        {
            var evt = (DomainEvents.TypeDefinitionRetiredEvent)e;
            return new TypeDefinitionRetiredEventSchema(evt.TypeDefinitionId.Value);
        });
    }
}
