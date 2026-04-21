using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

public sealed record RegisterParticipantCommand(
    Guid ParticipantId) : IHasAggregateId
{
    public Guid AggregateId => ParticipantId;
}

public sealed record PlaceParticipantCommand(
    Guid ParticipantId,
    Guid HomeClusterId,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => ParticipantId;
}
