using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Operator;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Operator;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalOperatorSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("OperatorCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.OperatorCreatedEvent), typeof(OperatorCreatedEventSchema));

        sink.RegisterPayloadMapper("OperatorCreatedEvent", e =>
        {
            var evt = (DomainEvents.OperatorCreatedEvent)e;
            return new OperatorCreatedEventSchema(evt.OperatorId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
