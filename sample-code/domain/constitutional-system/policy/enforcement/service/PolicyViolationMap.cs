namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

/// <summary>
/// Deterministic mapping: Violation severity → EnforcementType + EnforcementSeverity.
/// Pure domain logic — no infra, no randomness.
/// </summary>
public static class PolicyViolationMap
{
    public static (EnforcementType Type, EnforcementSeverity Severity) Resolve(string violationSeverity)
    {
        return violationSeverity.ToLowerInvariant() switch
        {
            "info" => (EnforcementType.Warning, EnforcementSeverity.Soft),
            "warning" => (EnforcementType.Restriction, EnforcementSeverity.Medium),
            "critical" => (EnforcementType.Freeze, EnforcementSeverity.Hard),
            "fatal" => (EnforcementType.Block, EnforcementSeverity.Critical),
            _ => (EnforcementType.Warning, EnforcementSeverity.Soft)
        };
    }

    public static EnforcementTargetType ResolveTarget(string classification)
    {
        return classification.ToLowerInvariant() switch
        {
            "identity" => EnforcementTargetType.Identity,
            "economic" => EnforcementTargetType.Wallet,
            "governance" => EnforcementTargetType.System,
            "operational" => EnforcementTargetType.Operator,
            _ => EnforcementTargetType.System
        };
    }
}
