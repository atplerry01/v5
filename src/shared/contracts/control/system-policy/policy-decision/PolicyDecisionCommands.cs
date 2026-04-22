using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyDecision;

public sealed record RecordPolicyDecisionCommand(
    Guid DecisionId,
    string PolicyDefinitionId,
    string SubjectId,
    string Action,
    string Resource,
    string Outcome,
    DateTimeOffset DecidedAt) : IHasAggregateId
{
    public Guid AggregateId => DecisionId;
}
