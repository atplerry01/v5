using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Participant;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

namespace Whycespace.Projections.Structural.Humancapital.Participant.Reducer;

public static class ParticipantProjectionReducer
{
    public static ParticipantReadModel Apply(ParticipantReadModel state, ParticipantRegisteredEventSchema e, DateTimeOffset at) =>
        state with
        {
            ParticipantId = e.AggregateId,
            LastModifiedAt = at
        };

    public static ParticipantReadModel Apply(ParticipantReadModel state, ParticipantPlacedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ParticipantId = e.AggregateId,
            HomeClusterId = e.HomeCluster,
            PlacedAt = e.EffectiveAt,
            LastModifiedAt = at
        };
}
