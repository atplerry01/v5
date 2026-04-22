namespace Whycespace.Shared.Contracts.Events.Control.SystemPolicy.PolicyEnforcement;

public sealed record PolicyEnforcedEventSchema(
    Guid AggregateId,
    string PolicyDecisionId,
    string TargetId,
    string Outcome,
    DateTimeOffset EnforcedAt,
    bool IsNoPolicyFlagAnomaly);
