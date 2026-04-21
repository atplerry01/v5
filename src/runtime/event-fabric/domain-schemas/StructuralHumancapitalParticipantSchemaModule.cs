using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Participant;
using DomainEvents = Whycespace.Domain.StructuralSystem.Humancapital.Participant;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalParticipantSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ParticipantRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.ParticipantRegisteredEvent), typeof(ParticipantRegisteredEventSchema));
        sink.RegisterSchema("ParticipantPlacedEvent", EventVersion.Default,
            typeof(DomainEvents.ParticipantPlacedEvent), typeof(ParticipantPlacedEventSchema));

        sink.RegisterPayloadMapper("ParticipantRegisteredEvent", e =>
        {
            var evt = (DomainEvents.ParticipantRegisteredEvent)e;
            return new ParticipantRegisteredEventSchema(Guid.Parse(evt.ParticipantId));
        });
        sink.RegisterPayloadMapper("ParticipantPlacedEvent", e =>
        {
            var evt = (DomainEvents.ParticipantPlacedEvent)e;
            return new ParticipantPlacedEventSchema(Guid.Parse(evt.ParticipantId), evt.HomeCluster.Value, evt.EffectiveAt);
        });
    }
}
