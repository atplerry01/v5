namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Participant;

public sealed record ParticipantRegisteredEventSchema(
    Guid AggregateId);

public sealed record ParticipantPlacedEventSchema(
    Guid AggregateId,
    Guid HomeCluster,
    DateTimeOffset EffectiveAt);
