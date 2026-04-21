using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Reputation;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Reputation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalReputationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ReputationCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.ReputationCreatedEvent), typeof(ReputationCreatedEventSchema));

        sink.RegisterPayloadMapper("ReputationCreatedEvent", e =>
        {
            var evt = (DomainEvents.ReputationCreatedEvent)e;
            return new ReputationCreatedEventSchema(evt.ReputationId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
