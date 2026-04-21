namespace Whycespace.Shared.Contracts.Events.Structural.Humancapital.Assignment;

public sealed record AssignmentAssignedEventSchema(
    Guid AggregateId,
    Guid ParticipantId,
    Guid AuthorityId,
    DateTimeOffset EffectiveAt);
