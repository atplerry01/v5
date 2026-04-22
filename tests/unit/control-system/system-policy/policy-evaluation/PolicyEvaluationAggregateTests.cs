using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyEvaluation;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyEvaluation;

public sealed class PolicyEvaluationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyEvaluationId NewId(string seed) =>
        new(Hex64($"PolicyEvaluationTests:{seed}:eval"));

    [Fact]
    public void Record_RaisesPolicyEvaluationRecordedEvent()
    {
        var id = NewId("Record");

        var aggregate = PolicyEvaluationAggregate.Record(id, "pol-1", "actor-1", "create", "corr-1");

        var evt = Assert.IsType<PolicyEvaluationRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal("pol-1", evt.PolicyId);
        Assert.Equal("actor-1", evt.ActorId);
        Assert.Equal("create", evt.Action);
    }

    [Fact]
    public void Record_WithEmptyPolicyId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyEvaluationAggregate.Record(NewId("EmptyPolicy"), "", "actor", "action", "corr"));
    }

    [Fact]
    public void Record_WithEmptyActorId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyEvaluationAggregate.Record(NewId("EmptyActor"), "pol", "", "action", "corr"));
    }

    [Fact]
    public void IssueVerdict_RaisesPolicyEvaluationVerdictIssuedEvent()
    {
        var aggregate = PolicyEvaluationAggregate.Record(NewId("Verdict"), "pol-1", "actor-1", "create", "corr-1");
        aggregate.ClearDomainEvents();

        aggregate.IssueVerdict(EvaluationOutcome.Allow, "hash-abc123");

        var evt = Assert.IsType<PolicyEvaluationVerdictIssuedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(EvaluationOutcome.Allow, evt.Outcome);
        Assert.Equal("hash-abc123", evt.DecisionHash);
        Assert.Equal(EvaluationOutcome.Allow, aggregate.Outcome);
    }

    [Fact]
    public void IssueVerdict_Twice_Throws()
    {
        var aggregate = PolicyEvaluationAggregate.Record(NewId("DoubleVerdict"), "pol-1", "actor-1", "create", "corr-1");
        aggregate.IssueVerdict(EvaluationOutcome.Deny, "hash-1");

        Assert.ThrowsAny<Exception>(() => aggregate.IssueVerdict(EvaluationOutcome.Allow, "hash-2"));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[] { new PolicyEvaluationRecordedEvent(id, "pol", "actor", "action", "corr") };
        var aggregate = (PolicyEvaluationAggregate)Activator.CreateInstance(typeof(PolicyEvaluationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Null(aggregate.Outcome);
        Assert.Empty(aggregate.DomainEvents);
    }
}
