namespace Whycespace.Domain.ConstitutionalSystem.Policy.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyScopeAggregate : AggregateRoot
{
    public Guid PolicyRuleId { get; private set; }
    public ScopeType Type { get; private set; } = default!;
    public ScopeTarget Target { get; private set; } = default!;
    public bool IsInclusive { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private PolicyScopeAggregate() { }

    public static PolicyScopeAggregate Create(
        Guid scopeId,
        Guid policyRuleId,
        ScopeType type,
        ScopeTarget target,
        DateTimeOffset timestamp,
        bool isInclusive = true,
        int priority = 0)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(target);

        return new PolicyScopeAggregate
        {
            Id = scopeId,
            PolicyRuleId = policyRuleId,
            Type = type,
            Target = target,
            IsInclusive = isInclusive,
            Priority = priority,
            CreatedAt = timestamp
        };
    }

    public bool Applies(string scopeType, string targetIdentifier)
    {
        if (Type.Value != scopeType) return false;
        return Target.Matches(targetIdentifier);
    }

    public void UpdateTarget(ScopeTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        Target = target;
    }

    public void SetPriority(int priority)
    {
        if (priority < 0) throw new ArgumentOutOfRangeException(nameof(priority));
        Priority = priority;
    }
}
