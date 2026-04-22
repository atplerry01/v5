using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.DispatchRule;

public sealed class DispatchRuleAggregate : AggregateRoot
{
    public DispatchRuleId DispatchRuleId { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public Guid RouteRef { get; private set; }
    public DispatchCondition Condition { get; private set; } = null!;
    public int Priority { get; private set; }
    public DispatchRuleStatus Status { get; private set; }

    private DispatchRuleAggregate() { }

    public static DispatchRuleAggregate Register(
        DispatchRuleId id,
        string ruleName,
        Guid routeRef,
        DispatchCondition condition,
        int priority,
        Timestamp registeredAt)
    {
        var aggregate = new DispatchRuleAggregate();
        if (aggregate.Version >= 0)
            throw DispatchRuleErrors.AlreadyInitialized();

        if (string.IsNullOrWhiteSpace(ruleName))
            throw DispatchRuleErrors.RuleNameMissing();

        if (routeRef == Guid.Empty)
            throw DispatchRuleErrors.RouteRefMissing();

        if (priority < 0)
            throw DispatchRuleErrors.PriorityNegative();

        aggregate.RaiseDomainEvent(new DispatchRuleRegisteredEvent(
            id, ruleName, routeRef, condition, priority, registeredAt));

        return aggregate;
    }

    public void Deactivate(Timestamp deactivatedAt)
    {
        if (Status == DispatchRuleStatus.Inactive)
            throw DispatchRuleErrors.AlreadyInactive();

        RaiseDomainEvent(new DispatchRuleDeactivatedEvent(DispatchRuleId, deactivatedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DispatchRuleRegisteredEvent e:
                DispatchRuleId = e.DispatchRuleId;
                RuleName = e.RuleName;
                RouteRef = e.RouteRef;
                Condition = e.Condition;
                Priority = e.Priority;
                Status = DispatchRuleStatus.Active;
                break;

            case DispatchRuleDeactivatedEvent:
                Status = DispatchRuleStatus.Inactive;
                break;
        }
    }
}
