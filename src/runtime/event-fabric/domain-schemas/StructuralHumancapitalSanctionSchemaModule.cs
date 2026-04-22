using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sanction;
using DomainEvents = Whycespace.Domain.ControlSystem.Enforcement.HumancapitalSanction;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalSanctionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SanctionCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.SanctionCreatedEvent), typeof(SanctionCreatedEventSchema));

        sink.RegisterPayloadMapper("SanctionCreatedEvent", e =>
        {
            var evt = (DomainEvents.SanctionCreatedEvent)e;
            return new SanctionCreatedEventSchema(evt.SanctionId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
