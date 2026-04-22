using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Performance;
using DomainEvents = Whycespace.Domain.DecisionSystem.Evaluation.Performance;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalPerformanceSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PerformanceCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.PerformanceCreatedEvent), typeof(PerformanceCreatedEventSchema));

        sink.RegisterPayloadMapper("PerformanceCreatedEvent", e =>
        {
            var evt = (DomainEvents.PerformanceCreatedEvent)e;
            return new PerformanceCreatedEventSchema(evt.PerformanceId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
