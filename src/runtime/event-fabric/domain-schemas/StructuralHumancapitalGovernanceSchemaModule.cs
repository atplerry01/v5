using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Governance;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Governance;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalGovernanceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("GovernanceCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.GovernanceCreatedEvent), typeof(GovernanceCreatedEventSchema));

        sink.RegisterPayloadMapper("GovernanceCreatedEvent", e =>
        {
            var evt = (DomainEvents.GovernanceCreatedEvent)e;
            return new GovernanceCreatedEventSchema(evt.GovernanceId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
