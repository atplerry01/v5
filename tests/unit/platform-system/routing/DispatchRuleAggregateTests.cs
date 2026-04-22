using Whycespace.Domain.PlatformSystem.Routing.DispatchRule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Routing;

public sealed class DispatchRuleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Guid SomeRouteRef = new("11111111-1111-1111-1111-111111111111");

    private static DispatchRuleAggregate NewRegistered(string seed)
    {
        var id = new DispatchRuleId(IdGen.Generate($"DispatchRuleAggregateTests:{seed}"));
        var condition = new DispatchCondition(DispatchConditionType.MessageKindMatch, "Command");
        return DispatchRuleAggregate.Register(id, "RouteByKind", SomeRouteRef, condition, 10, Now);
    }

    [Fact]
    public void Register_WithValidArgs_RaisesDispatchRuleRegisteredEvent()
    {
        var aggregate = NewRegistered("Register");

        Assert.Equal(DispatchRuleStatus.Active, aggregate.Status);
        Assert.Equal("RouteByKind", aggregate.RuleName);
        Assert.Equal(SomeRouteRef, aggregate.RouteRef);
        Assert.Equal(10, aggregate.Priority);

        var evt = Assert.IsType<DispatchRuleRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("MessageKindMatch", evt.Condition.ConditionType.Value);
        Assert.Equal("Command", evt.Condition.MatchValue);
    }

    [Fact]
    public void Deactivate_FromActive_TransitionsToInactive()
    {
        var aggregate = NewRegistered("Deactivate");
        aggregate.ClearDomainEvents();

        aggregate.Deactivate(Now);

        Assert.Equal(DispatchRuleStatus.Inactive, aggregate.Status);
        Assert.IsType<DispatchRuleDeactivatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_Throws()
    {
        var aggregate = NewRegistered("DoubleDeactivate");
        aggregate.Deactivate(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Deactivate(Now));
    }

    [Fact]
    public void Register_WithEmptyRuleName_Throws()
    {
        var id = new DispatchRuleId(IdGen.Generate("DispatchRuleAggregateTests:EmptyName"));
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);

        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(id, "", SomeRouteRef, condition, 5, Now));
    }

    [Fact]
    public void Register_WithNegativePriority_Throws()
    {
        var id = new DispatchRuleId(IdGen.Generate("DispatchRuleAggregateTests:NegPriority"));
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);

        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(id, "Rule", SomeRouteRef, condition, -1, Now));
    }

    [Fact]
    public void Register_WithEmptyRouteRef_Throws()
    {
        var id = new DispatchRuleId(IdGen.Generate("DispatchRuleAggregateTests:EmptyRoute"));
        var condition = new DispatchCondition(DispatchConditionType.AlwaysMatch, string.Empty);

        Assert.Throws<DomainInvariantViolationException>(() =>
            DispatchRuleAggregate.Register(id, "Rule", Guid.Empty, condition, 5, Now));
    }

    [Fact]
    public void DispatchCondition_AlwaysMatch_WithNonEmptyMatchValue_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new DispatchCondition(DispatchConditionType.AlwaysMatch, "non-empty"));
    }
}
