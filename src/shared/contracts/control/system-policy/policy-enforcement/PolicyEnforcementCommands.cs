using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;

public sealed record RecordPolicyEnforcementCommand(
    Guid EnforcementId,
    string PolicyDecisionId,
    string TargetId,
    string Outcome,
    DateTimeOffset EnforcedAt,
    bool IsNoPolicyFlagAnomaly = false) : IHasAggregateId
{
    public Guid AggregateId => EnforcementId;
}
