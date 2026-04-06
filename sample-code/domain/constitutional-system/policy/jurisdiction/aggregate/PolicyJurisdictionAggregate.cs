using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Jurisdiction;

/// <summary>
/// Jurisdiction-aware policy overlay. Manages region-specific governance rules
/// that extend or override base policies based on jurisdiction.
/// Enforces conflict resolution when multiple jurisdictions apply.
/// </summary>
public sealed class PolicyJurisdictionAggregate : AggregateRoot
{
    public string JurisdictionCode { get; private set; } = string.Empty;
    public string RegionId { get; private set; } = string.Empty;
    public JurisdictionPolicyStatus Status { get; private set; } = JurisdictionPolicyStatus.Draft;
    private readonly List<PolicyOverlayRule> _overlayRules = [];
    public IReadOnlyList<PolicyOverlayRule> OverlayRules => _overlayRules.AsReadOnly();

    public static PolicyJurisdictionAggregate Create(Guid id, string jurisdictionCode, string regionId)
    {
        var agg = new PolicyJurisdictionAggregate
        {
            Id = id,
            JurisdictionCode = jurisdictionCode,
            RegionId = regionId,
            Status = JurisdictionPolicyStatus.Draft
        };
        agg.RaiseDomainEvent(new JurisdictionPolicyCreatedEvent(id, jurisdictionCode, regionId));
        return agg;
    }

    public void AddOverlayRule(PolicyOverlayRule rule)
    {
        EnsureNotTerminal(Status, s => s.IsTerminal, "AddOverlayRule");
        EnsureInvariant(
            !_overlayRules.Any(r => r.PolicyAction == rule.PolicyAction),
            "UniqueOverlayPerAction",
            $"An overlay rule for action '{rule.PolicyAction}' already exists.");
        _overlayRules.Add(rule);
        RaiseDomainEvent(new OverlayRuleAddedEvent(Id, rule.PolicyAction, rule.Effect, rule.Priority));
    }

    public void Activate()
    {
        EnsureValidTransition(Status, JurisdictionPolicyStatus.Active, JurisdictionPolicyStatus.IsValidTransition);
        EnsureInvariant(_overlayRules.Count > 0, "MustHaveRules", "Cannot activate without overlay rules.");
        Status = JurisdictionPolicyStatus.Active;
        RaiseDomainEvent(new JurisdictionPolicyActivatedEvent(Id));
    }

    public void Suspend()
    {
        EnsureValidTransition(Status, JurisdictionPolicyStatus.Suspended, JurisdictionPolicyStatus.IsValidTransition);
        Status = JurisdictionPolicyStatus.Suspended;
        RaiseDomainEvent(new JurisdictionPolicySuspendedEvent(Id));
    }

    public void Retire()
    {
        EnsureValidTransition(Status, JurisdictionPolicyStatus.Retired, JurisdictionPolicyStatus.IsValidTransition);
        Status = JurisdictionPolicyStatus.Retired;
        RaiseDomainEvent(new JurisdictionPolicyRetiredEvent(Id));
    }
}
