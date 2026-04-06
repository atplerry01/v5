using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed class PolicyAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public PolicyStatus Status { get; private set; } = default!;
    public Guid ScopeId { get; private set; }
    public PolicyPriority Priority { get; private set; } = PolicyPriority.Default;
    public Guid ActiveVersionId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<Guid> _ruleIds = [];
    public IReadOnlyList<Guid> RuleIds => _ruleIds.AsReadOnly();

    private readonly List<Guid> _versionIds = [];
    public IReadOnlyList<Guid> VersionIds => _versionIds.AsReadOnly();

    private PolicyAggregate() { }

    public static PolicyAggregate Create(
        Guid policyId,
        string name,
        string description,
        Guid scopeId,
        DateTimeOffset timestamp,
        PolicyPriority? priority = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var policy = new PolicyAggregate
        {
            Id = policyId,
            Name = name,
            Description = description,
            ScopeId = scopeId,
            Priority = priority ?? PolicyPriority.Default,
            Status = PolicyStatus.Draft,
            CreatedAt = timestamp
        };

        policy.RaiseDomainEvent(new PolicyCreatedEvent(policyId, name, scopeId));
        return policy;
    }

    public void AddRule(Guid ruleId)
    {
        EnsureInvariant(!_ruleIds.Contains(ruleId), "UniqueRule", "Rule already attached to this policy.");
        _ruleIds.Add(ruleId);
        RaiseDomainEvent(new PolicyRuleAttachedEvent(Id, ruleId));
    }

    public void RemoveRule(Guid ruleId)
    {
        EnsureInvariant(_ruleIds.Contains(ruleId), "RuleExists", "Rule is not attached to this policy.");
        _ruleIds.Remove(ruleId);
    }

    public void SetActiveVersion(Guid versionId)
    {
        if (!_versionIds.Contains(versionId))
            _versionIds.Add(versionId);

        ActiveVersionId = versionId;
        RaiseDomainEvent(new PolicyVersionSetEvent(Id, versionId));
    }

    public void Activate()
    {
        EnsureInvariant(Status == PolicyStatus.Draft, "DraftOnly", "Only draft policies can be activated.");
        EnsureInvariant(ScopeId != Guid.Empty, "ScopeRequired", "Policy must have a scope before activation.");
        EnsureInvariant(_ruleIds.Count > 0, "RulesRequired", "Policy must have at least one rule.");

        Status = PolicyStatus.Active;
        RaiseDomainEvent(new PolicyActivatedEvent(Id));
    }

    public void Suspend()
    {
        EnsureInvariant(Status == PolicyStatus.Active, "ActiveOnly", "Only active policies can be suspended.");
        Status = PolicyStatus.Suspended;
        RaiseDomainEvent(new PolicySuspendedEvent(Id));
    }

    public void Archive()
    {
        EnsureNotTerminal(Status, s => s == PolicyStatus.Archived, "Archive");
        Status = PolicyStatus.Archived;
        RaiseDomainEvent(new PolicyArchivedEvent(Id));
    }
}
