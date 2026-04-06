using Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;
using Whycespace.Shared.Contracts.Domain.Constitutional.Policy;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IPolicyViolationMapping — bridges to domain PolicyViolationMap.
/// </summary>
public sealed class PolicyViolationMappingService : IPolicyViolationMapping
{
    public (string EnforcementType, string EnforcementSeverity) Resolve(string violationSeverity)
    {
        var (type, severity) = PolicyViolationMap.Resolve(violationSeverity);
        return (type.ToString(), severity.ToString());
    }

    public string ResolveTarget(string classification)
    {
        return PolicyViolationMap.ResolveTarget(classification).ToString();
    }

    public bool IsCritical(string severity)
    {
        return string.Equals(severity, EnforcementSeverity.Critical.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
