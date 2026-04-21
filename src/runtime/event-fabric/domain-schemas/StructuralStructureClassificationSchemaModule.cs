using Whycespace.Shared.Contracts.Events.Structural.Structure.Classification;
using DomainEvents = Whycespace.Domain.StructuralSystem.Structure.Classification;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralStructureClassificationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ClassificationDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.ClassificationDefinedEvent), typeof(ClassificationDefinedEventSchema));
        sink.RegisterSchema("ClassificationActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.ClassificationActivatedEvent), typeof(ClassificationActivatedEventSchema));
        sink.RegisterSchema("ClassificationDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.ClassificationDeprecatedEvent), typeof(ClassificationDeprecatedEventSchema));

        sink.RegisterPayloadMapper("ClassificationDefinedEvent", e =>
        {
            var evt = (DomainEvents.ClassificationDefinedEvent)e;
            return new ClassificationDefinedEventSchema(evt.ClassificationId.Value, evt.Descriptor.ClassificationName, evt.Descriptor.ClassificationCategory);
        });
        sink.RegisterPayloadMapper("ClassificationActivatedEvent", e =>
        {
            var evt = (DomainEvents.ClassificationActivatedEvent)e;
            return new ClassificationActivatedEventSchema(evt.ClassificationId.Value);
        });
        sink.RegisterPayloadMapper("ClassificationDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.ClassificationDeprecatedEvent)e;
            return new ClassificationDeprecatedEventSchema(evt.ClassificationId.Value);
        });
    }
}
