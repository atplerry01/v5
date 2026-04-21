using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;

public sealed record CreateAssignmentCommand(
    Guid AssignmentId,
    Guid GrantId,
    Guid SubjectId,
    string Scope) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}

public sealed record ActivateAssignmentCommand(
    Guid AssignmentId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}

public sealed record RevokeAssignmentCommand(
    Guid AssignmentId,
    DateTimeOffset RevokedAt) : IHasAggregateId
{
    public Guid AggregateId => AssignmentId;
}
