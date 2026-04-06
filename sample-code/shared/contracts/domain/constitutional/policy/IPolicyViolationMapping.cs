namespace Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

/// <summary>
/// Engine-facing policy violation mapping contract.
/// Replaces direct import of domain PolicyViolationMap, EnforcementSeverity, EnforcementType, EnforcementTargetType.
/// </summary>
public interface IPolicyViolationMapping
{
    (string EnforcementType, string EnforcementSeverity) Resolve(string violationSeverity);
    string ResolveTarget(string classification);
    bool IsCritical(string severity);
}
