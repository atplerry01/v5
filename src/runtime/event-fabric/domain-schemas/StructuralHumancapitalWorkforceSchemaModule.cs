using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Workforce;
using DomainEvents = Whycespace.Domain.BusinessSystem.Workforce.Workforce;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalWorkforceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("WorkforceCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.WorkforceCreatedEvent), typeof(WorkforceCreatedEventSchema));

        sink.RegisterPayloadMapper("WorkforceCreatedEvent", e =>
        {
            var evt = (DomainEvents.WorkforceCreatedEvent)e;
            return new WorkforceCreatedEventSchema(evt.WorkforceId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
