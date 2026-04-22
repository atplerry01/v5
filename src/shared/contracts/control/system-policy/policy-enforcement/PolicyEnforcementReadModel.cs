namespace Whycespace.Shared.Contracts.Control.SystemPolicy.PolicyEnforcement;

public sealed record PolicyEnforcementReadModel
{
    public Guid EnforcementId { get; init; }
    public string PolicyDecisionId { get; init; } = string.Empty;
    public string TargetId { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public DateTimeOffset EnforcedAt { get; init; }
    public bool IsNoPolicyFlagAnomaly { get; init; }
}
