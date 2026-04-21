using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Assignment;
using Whycespace.Shared.Contracts.Structural.Humancapital.Assignment;

namespace Whycespace.Projections.Structural.Humancapital.Assignment.Reducer;

public static class AssignmentProjectionReducer
{
    public static AssignmentReadModel Apply(AssignmentReadModel state, AssignmentAssignedEventSchema e, DateTimeOffset at) =>
        state with
        {
            AssignmentId = e.AggregateId,
            ParticipantId = e.ParticipantId,
            AuthorityId = e.AuthorityId,
            EffectiveAt = e.EffectiveAt,
            LastModifiedAt = at
        };
}
