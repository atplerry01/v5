using Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Enforcement.Rule;

public sealed class EnforcementRuleAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static RuleId NewId(string seed) =>
        new(IdGen.Generate($"EnforcementRuleTests:{seed}:rule"));

    private static DocumentRef NewDocRef(string seed) =>
        new(new ContentId(IdGen.Generate($"EnforcementRuleTests:{seed}:doc")));

    private static EnforcementRuleAggregate Define(string seed) =>
        EnforcementRuleAggregate.Define(
            NewId(seed),
            new RuleCode("RULE-001"),
            new RuleName("Capital Limit Rule"),
            new RuleCategory("Capital"),
            RuleScope.Capital,
            RuleSeverity.High,
            NewDocRef(seed),
            BaseTime);

    [Fact]
    public void Define_RaisesEnforcementRuleDefinedEvent()
    {
        var id = NewId("Define_Valid");
        var aggregate = EnforcementRuleAggregate.Define(
            id,
            new RuleCode("CAP-001"),
            new RuleName("Max Capital Rule"),
            new RuleCategory("Capital"),
            RuleScope.Capital,
            RuleSeverity.Critical,
            NewDocRef("Define_Valid"),
            BaseTime);

        var evt = Assert.IsType<EnforcementRuleDefinedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RuleId);
        Assert.Equal("CAP-001", evt.RuleCode.Value);
        Assert.Equal("Max Capital Rule", evt.RuleName);
    }

    [Fact]
    public void Define_SetsStatusToActive()
    {
        var aggregate = Define("State");

        Assert.Equal(RuleStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Define_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var doc = NewDocRef("Stable");
        var r1 = EnforcementRuleAggregate.Define(id, new RuleCode("R1"), new RuleName("Rule A"), new RuleCategory("Cap"), RuleScope.Capital, RuleSeverity.Low, doc, BaseTime);
        var r2 = EnforcementRuleAggregate.Define(id, new RuleCode("R1"), new RuleName("Rule A"), new RuleCategory("Cap"), RuleScope.Capital, RuleSeverity.Low, doc, BaseTime);

        Assert.Equal(
            ((EnforcementRuleDefinedEvent)r1.DomainEvents[0]).RuleId.Value,
            ((EnforcementRuleDefinedEvent)r2.DomainEvents[0]).RuleId.Value);
    }

    [Fact]
    public void Disable_FromActive_SetsStatusToDisabled()
    {
        var aggregate = Define("Disable");
        aggregate.ClearDomainEvents();

        aggregate.Disable();

        Assert.IsType<EnforcementRuleDisabledEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RuleStatus.Disabled, aggregate.Status);
    }

    [Fact]
    public void Retire_SetsStatusToRetired()
    {
        var aggregate = Define("Retire");
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<EnforcementRuleRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(RuleStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Activate_AfterRetire_Throws()
    {
        var aggregate = Define("Activate_Retired");
        aggregate.Retire();

        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var doc = NewDocRef("History");

        var history = new object[]
        {
            new EnforcementRuleDefinedEvent(id, new RuleCode("H-001"), "History Rule", new RuleCategory("Ledger"), RuleScope.Ledger, RuleSeverity.Medium, doc, BaseTime)
        };

        var aggregate = (EnforcementRuleAggregate)Activator.CreateInstance(typeof(EnforcementRuleAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.RuleId);
        Assert.Equal(RuleStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
