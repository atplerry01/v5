namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

/// <summary>
/// Domain service for jurisdiction policy conflict resolution.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class JurisdictionPolicyService
{
    public PolicyOverlayRule? ResolveConflict(
        IReadOnlyList<PolicyOverlayRule> rules,
        ConflictResolutionStrategy strategy)
    {
        if (rules.Count == 0) return null;
        if (rules.Count == 1) return rules[0];

        return strategy.Value switch
        {
            "MostRestrictive" => rules.OrderByDescending(r => r.Effect switch
            {
                OverlayEffect.Deny => 4,
                OverlayEffect.Restrict => 3,
                OverlayEffect.RequireAdditionalApproval => 2,
                OverlayEffect.Allow => 1,
                _ => 0
            }).First(),
            "HighestPriority" => rules.OrderByDescending(r => r.Priority).First(),
            "LocalOverridesGlobal" => rules.OrderByDescending(r => r.Priority).First(),
            _ => rules.OrderByDescending(r => r.Priority).First()
        };
    }

    public bool CanActivate(PolicyJurisdictionAggregate policy) =>
        policy.Status == JurisdictionPolicyStatus.Draft && policy.OverlayRules.Count > 0;
}
