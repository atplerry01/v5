using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Eligibility;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Eligibility;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalEligibilitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("EligibilityCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.EligibilityCreatedEvent), typeof(EligibilityCreatedEventSchema));

        sink.RegisterPayloadMapper("EligibilityCreatedEvent", e =>
        {
            var evt = (DomainEvents.EligibilityCreatedEvent)e;
            return new EligibilityCreatedEventSchema(evt.EligibilityId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
