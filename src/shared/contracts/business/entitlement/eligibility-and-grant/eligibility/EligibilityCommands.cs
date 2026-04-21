using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;

public sealed record CreateEligibilityCommand(
    Guid EligibilityId,
    Guid SubjectId,
    Guid TargetId,
    string Scope) : IHasAggregateId
{
    public Guid AggregateId => EligibilityId;
}

public sealed record MarkEligibleCommand(
    Guid EligibilityId,
    DateTimeOffset EvaluatedAt) : IHasAggregateId
{
    public Guid AggregateId => EligibilityId;
}

public sealed record MarkIneligibleCommand(
    Guid EligibilityId,
    string Reason,
    DateTimeOffset EvaluatedAt) : IHasAggregateId
{
    public Guid AggregateId => EligibilityId;
}
