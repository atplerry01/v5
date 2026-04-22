using Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemPolicy.PolicyAudit;

public sealed class PolicyAuditAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset BaseTime = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static PolicyAuditId NewId(string seed) =>
        new(Hex64($"PolicyAuditTests:{seed}:audit"));

    private static PolicyAuditAggregate RecordDefault(string seed) =>
        PolicyAuditAggregate.Record(
            NewId(seed), "pol-1", "actor-1", "create",
            PolicyAuditCategory.EvaluationPass, "hash-abc", "corr-1", BaseTime);

    [Fact]
    public void Record_RaisesPolicyAuditEntryRecordedEvent()
    {
        var id = NewId("Record");

        var aggregate = PolicyAuditAggregate.Record(
            id, "pol-1", "actor-1", "create",
            PolicyAuditCategory.EvaluationPass, "hash-abc", "corr-1", BaseTime);

        var evt = Assert.IsType<PolicyAuditEntryRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(PolicyAuditCategory.EvaluationPass, evt.Category);
        Assert.Equal("pol-1", evt.PolicyId);
        Assert.Equal(BaseTime, evt.OccurredAt);
    }

    [Fact]
    public void Record_SetsIsReviewedFalse()
    {
        var aggregate = RecordDefault("State");

        Assert.False(aggregate.IsReviewed);
        Assert.Null(aggregate.ReviewerId);
    }

    [Fact]
    public void Record_WithEmptyPolicyId_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            PolicyAuditAggregate.Record(NewId("EmptyPol"), "", "actor", "create",
                PolicyAuditCategory.EvaluationPass, "hash", "corr", BaseTime));
    }

    [Fact]
    public void Review_RaisesPolicyAuditEntryReviewedEvent()
    {
        var aggregate = RecordDefault("Review");
        aggregate.ClearDomainEvents();

        aggregate.Review("reviewer-1", "Looks compliant.");

        var evt = Assert.IsType<PolicyAuditEntryReviewedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("reviewer-1", evt.ReviewerId);
        Assert.True(aggregate.IsReviewed);
        Assert.Equal("reviewer-1", aggregate.ReviewerId);
    }

    [Fact]
    public void Review_AlreadyReviewed_Throws()
    {
        var aggregate = RecordDefault("DoubleReview");
        aggregate.Review("reviewer-1", "First review.");

        Assert.ThrowsAny<Exception>(() => aggregate.Review("reviewer-2", "Second review."));
    }

    [Fact]
    public void Review_WithEmptyReason_Throws()
    {
        var aggregate = RecordDefault("EmptyReason");

        Assert.ThrowsAny<Exception>(() => aggregate.Review("reviewer-1", ""));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new PolicyAuditEntryRecordedEvent(id, "pol", "actor", "action",
                PolicyAuditCategory.EvaluationDeny, "hash", "corr", BaseTime)
        };
        var aggregate = (PolicyAuditAggregate)Activator.CreateInstance(typeof(PolicyAuditAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.False(aggregate.IsReviewed);
        Assert.Empty(aggregate.DomainEvents);
    }
}
