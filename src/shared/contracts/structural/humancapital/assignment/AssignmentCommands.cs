using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

public sealed record AssignAssignmentCommand(
    Guid AssignmentId,
    Guid ParticipantId,
    Guid AuthorityId,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}
