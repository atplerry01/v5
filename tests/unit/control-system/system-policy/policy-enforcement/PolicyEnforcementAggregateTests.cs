using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEnforcement;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyEnforcement;

public sealed class PolicyEnforcementAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyEnforcementId NewId(string seed) =>
        new(Hex64($"PolicyEnforcementTests:{seed}:enforcement"));

    [Fact]
    public void Record_RaisesPolicyEnforcedEvent()
    {
        var id = NewId("Record");

        var aggregate = PolicyEnforcementAggregate.Record(id, "decision-1", "target-1", PolicyEnforcementOutcome.Enforced, BaseTime);

        var evt = Assert.IsType<PolicyEnforcedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(PolicyEnforcementOutcome.Enforced, evt.Outcome);
        Assert.Equal("decision-1", evt.PolicyDecisionId);
        Assert.False(evt.IsNoPolicyFlagAnomaly);
    }

    [Fact]
    public void Record_SetsProperties()
    {
        var id = NewId("State");

        var aggregate = PolicyEnforcementAggregate.Record(id, "decision-1", "target-1", PolicyEnforcementOutcome.Enforced, BaseTime);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyEnforcementOutcome.Enforced, aggregate.Outcome);
        Assert.False(aggregate.IsNoPolicyFlagAnomaly);
    }

    [Fact]
    public void Record_NoPolicyFlagAnomaly_WithBypassedOutcome_Succeeds()
    {
        var aggregate = PolicyEnforcementAggregate.Record(
            NewId("Anomaly"), "decision-1", "target-1",
            PolicyEnforcementOutcome.Bypassed, BaseTime, isNoPolicyFlagAnomaly: true);

        Assert.True(aggregate.IsNoPolicyFlagAnomaly);
        Assert.Equal(PolicyEnforcementOutcome.Bypassed, aggregate.Outcome);
    }

    [Fact]
    public void Record_NoPolicyFlagAnomaly_WithNonBypassedOutcome_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyEnforcementAggregate.Record(
                NewId("AnomalyInvalid"), "decision-1", "target-1",
                PolicyEnforcementOutcome.Enforced, BaseTime, isNoPolicyFlagAnomaly: true));
    }

    [Fact]
    public void Record_WithEmptyDecisionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyEnforcementAggregate.Record(NewId("EmptyDecision"), "", "target", PolicyEnforcementOutcome.Enforced, BaseTime));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new PolicyEnforcedEvent(id, "decision-1", "target-1", PolicyEnforcementOutcome.Enforced, BaseTime, false) };
        var aggregate = (PolicyEnforcementAggregate)Activator.CreateInstance(typeof(PolicyEnforcementAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyEnforcementOutcome.Enforced, aggregate.Outcome);
        Assert.Empty(aggregate.DomainEvents);
    }
}
