using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OperationalSystem.Routing.Path;

public sealed class RoutingPathAggregate : AggregateRoot
{
    public PathId PathId { get; private set; }
    public PathType PathType { get; private set; }
    public string Conditions { get; private set; } = string.Empty;
    public int Priority { get; private set; }
    public RoutingPathStatus Status { get; private set; }

    private RoutingPathAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static RoutingPathAggregate DefinePath(
        PathId pathId,
        PathType pathType,
        string conditions,
        int priority)
    {
        if (string.IsNullOrWhiteSpace(conditions))
            throw RoutingPathErrors.InvalidPathCondition();

        if (priority <= 0)
            throw RoutingPathErrors.InvalidPriority(priority);

        var aggregate = new RoutingPathAggregate();
        aggregate.RaiseDomainEvent(new RoutingPathDefinedEvent(
            pathId, pathType, conditions, priority));
        return aggregate;
    }

    // ── Activate ─────────────────────────────────────────────────

    public void Activate(Timestamp activatedAt)
    {
        var specification = new CanActivatePathSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw RoutingPathErrors.InvalidStateTransition(Status, RoutingPathStatus.Active);

        RaiseDomainEvent(new RoutingPathActivatedEvent(PathId, activatedAt));
    }

    // ── Disable ──────────────────────────────────────────────────

    public void Disable(Timestamp disabledAt)
    {
        var specification = new CanDisablePathSpecification();
        if (!specification.IsSatisfiedBy(this))
            throw RoutingPathErrors.InvalidStateTransition(Status, RoutingPathStatus.Disabled);

        RaiseDomainEvent(new RoutingPathDisabledEvent(PathId, disabledAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RoutingPathDefinedEvent e:
                PathId = e.PathId;
                PathType = e.PathType;
                Conditions = e.Conditions;
                Priority = e.Priority;
                Status = RoutingPathStatus.Defined;
                break;

            case RoutingPathActivatedEvent:
                Status = RoutingPathStatus.Active;
                break;

            case RoutingPathDisabledEvent:
                Status = RoutingPathStatus.Disabled;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (string.IsNullOrWhiteSpace(Conditions))
            throw RoutingPathErrors.ConditionsMustNotBeEmpty();

        if (Priority <= 0)
            throw RoutingPathErrors.PriorityMustBePositive();
    }
}
