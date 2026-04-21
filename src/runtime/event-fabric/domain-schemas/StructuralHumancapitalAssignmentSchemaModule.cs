using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Assignment;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Assignment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalAssignmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AssignmentAssignedEvent", EventVersion.Default,
            typeof(DomainEvents.AssignmentAssignedEvent), typeof(AssignmentAssignedEventSchema));

        sink.RegisterPayloadMapper("AssignmentAssignedEvent", e =>
        {
            var evt = (DomainEvents.AssignmentAssignedEvent)e;
            return new AssignmentAssignedEventSchema(
                evt.AssignmentId.Value,
                Guid.Parse(evt.Participant.Value),
                evt.Authority.Value,
                evt.EffectiveAt);
        });
    }
}
