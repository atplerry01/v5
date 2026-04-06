namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

/// <summary>
/// Specification: jurisdiction policy must have at least one overlay rule
/// and must be in Active status to be applied.
/// </summary>
public sealed class JurisdictionOverlaySpecification
{
    public bool IsSatisfiedBy(PolicyJurisdictionAggregate policy) =>
        policy.Status == JurisdictionPolicyStatus.Active && policy.OverlayRules.Count > 0;
}
