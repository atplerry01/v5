using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Incentive;
using DomainEvents = Whycespace.Domain.BusinessSystem.Workforce.Incentive;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalIncentiveSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("IncentiveCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.IncentiveCreatedEvent), typeof(IncentiveCreatedEventSchema));

        sink.RegisterPayloadMapper("IncentiveCreatedEvent", e =>
        {
            var evt = (DomainEvents.IncentiveCreatedEvent)e;
            return new IncentiveCreatedEventSchema(evt.IncentiveId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
