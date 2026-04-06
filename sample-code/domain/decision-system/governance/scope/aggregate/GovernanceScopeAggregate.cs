namespace Whycespace.Domain.DecisionSystem.Governance.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class GovernanceScopeAggregate : AggregateRoot
{
    public ScopeId ScopeId { get; private set; }
    public GovernanceScopeType ScopeType { get; private set; } = default!;
    public Guid TargetId { get; private set; }
    public AuthorityLevel AuthorityLevel { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private GovernanceScopeAggregate() { }

    public static GovernanceScopeAggregate DefineScope(
        Guid scopeId,
        GovernanceScopeType scopeType,
        Guid targetId,
        AuthorityLevel authorityLevel,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(scopeType);
        ArgumentNullException.ThrowIfNull(authorityLevel);

        if (scopeId == Guid.Empty)
            throw new ArgumentException("Scope ID must be specified.", nameof(scopeId));

        if (targetId == Guid.Empty)
            throw new ArgumentException("Target ID must be specified.", nameof(targetId));

        var scope = new GovernanceScopeAggregate
        {
            Id = scopeId,
            ScopeType = scopeType,
            TargetId = targetId,
            AuthorityLevel = authorityLevel,
            CreatedAt = timestamp
        };

        scope.ScopeId = scope.Id;

        scope.RaiseDomainEvent(new GovernanceScopeDefinedEvent(
            scope.Id,
            scopeType.Value,
            targetId,
            authorityLevel.Value));

        return scope;
    }

    public void UpdateScope(GovernanceScopeType scopeType, AuthorityLevel authorityLevel, DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(scopeType);
        ArgumentNullException.ThrowIfNull(authorityLevel);

        ScopeType = scopeType;
        AuthorityLevel = authorityLevel;
        UpdatedAt = timestamp;
    }

    public bool AppliesToTarget(Guid targetId) => TargetId == targetId;

    public bool CoversScope(string policyScopeType)
    {
        return ScopeType.Value == policyScopeType
            || ScopeType == GovernanceScopeType.Global;
    }
}
