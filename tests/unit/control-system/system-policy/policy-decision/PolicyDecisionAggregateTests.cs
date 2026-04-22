using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyDecision;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyDecision;

public sealed class PolicyDecisionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyDecisionId NewId(string seed) =>
        new(Hex64($"PolicyDecisionTests:{seed}:decision"));

    [Fact]
    public void Record_RaisesPolicyDecisionRecordedEvent()
    {
        var id = NewId("Record");

        var aggregate = PolicyDecisionAggregate.Record(
            id, "policy-def-1", "subject-1", "create", "resource:docs",
            PolicyDecisionOutcome.Permit, BaseTime);

        var evt = Assert.IsType<PolicyDecisionRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(PolicyDecisionOutcome.Permit, evt.Outcome);
        Assert.Equal("subject-1", evt.SubjectId);
    }

    [Fact]
    public void Record_SetsAllProperties()
    {
        var id = NewId("State");

        var aggregate = PolicyDecisionAggregate.Record(
            id, "policy-def-1", "subject-1", "create", "resource:docs",
            PolicyDecisionOutcome.Deny, BaseTime);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyDecisionOutcome.Deny, aggregate.Outcome);
        Assert.Equal("subject-1", aggregate.SubjectId);
        Assert.Equal("create", aggregate.Action);
        Assert.Equal(BaseTime, aggregate.DecidedAt);
    }

    [Fact]
    public void Record_WithEmptySubjectId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyDecisionAggregate.Record(NewId("EmptySub"), "pol", "", "action", "res", PolicyDecisionOutcome.Permit, BaseTime));
    }

    [Fact]
    public void Record_WithEmptyPolicyDefinitionId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyDecisionAggregate.Record(NewId("EmptyPol"), "", "sub", "action", "res", PolicyDecisionOutcome.Permit, BaseTime));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new PolicyDecisionRecordedEvent(id, "pol-def", "sub", "action", "res", PolicyDecisionOutcome.Permit, BaseTime)
        };
        var aggregate = (PolicyDecisionAggregate)Activator.CreateInstance(typeof(PolicyDecisionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(PolicyDecisionOutcome.Permit, aggregate.Outcome);
        Assert.Empty(aggregate.DomainEvents);
    }
}
