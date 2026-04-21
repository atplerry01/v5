using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Stewardship;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Stewardship;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalStewardshipSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("StewardshipCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.StewardshipCreatedEvent), typeof(StewardshipCreatedEventSchema));

        sink.RegisterPayloadMapper("StewardshipCreatedEvent", e =>
        {
            var evt = (DomainEvents.StewardshipCreatedEvent)e;
            return new StewardshipCreatedEventSchema(evt.StewardshipId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
