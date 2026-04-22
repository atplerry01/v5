using Whycespace.Domain.ContentSystem.Document.Governance.Retention;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Document.Governance.Retention;

public sealed class RetentionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureTime = new(new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero));

    private static RetentionId NewId(string seed) =>
        new(IdGen.Generate($"RetentionAggregateTests:{seed}:retention"));

    [Fact]
    public void Apply_RaisesRetentionAppliedEvent()
    {
        var id = NewId("Apply_Valid");
        var targetRef = new RetentionTargetRef(IdGen.Generate("RetentionAggregateTests:target-ref"), RetentionTargetKind.Document);
        var window = new RetentionWindow(BaseTime, FutureTime);
        var reason = new RetentionReason("Legal hold for audit.");

        var aggregate = RetentionAggregate.Apply(id, targetRef, window, reason, BaseTime);

        var evt = Assert.IsType<RetentionAppliedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.RetentionId);
        Assert.Equal(targetRef, evt.TargetRef);
    }

    [Fact]
    public void Apply_SetsStatusToApplied()
    {
        var id = NewId("Apply_State");
        var targetRef = new RetentionTargetRef(IdGen.Generate("RetentionAggregateTests:target-state"), RetentionTargetKind.Record);
        var window = new RetentionWindow(BaseTime, FutureTime);

        var aggregate = RetentionAggregate.Apply(id, targetRef, window, new RetentionReason("Compliance."), BaseTime);

        Assert.Equal(id, aggregate.RetentionId);
        Assert.Equal(RetentionStatus.Applied, aggregate.Status);
    }

    [Fact]
    public void Apply_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var targetRef = new RetentionTargetRef(IdGen.Generate("RetentionAggregateTests:stable-target"), RetentionTargetKind.Document);
        var window = new RetentionWindow(BaseTime, FutureTime);
        var reason = new RetentionReason("Stable retention.");
        var r1 = RetentionAggregate.Apply(id, targetRef, window, reason, BaseTime);
        var r2 = RetentionAggregate.Apply(id, targetRef, window, reason, BaseTime);

        Assert.Equal(
            ((RetentionAppliedEvent)r1.DomainEvents[0]).RetentionId.Value,
            ((RetentionAppliedEvent)r2.DomainEvents[0]).RetentionId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesRetentionState()
    {
        var id = NewId("History");
        var targetRef = new RetentionTargetRef(IdGen.Generate("RetentionAggregateTests:history-target"), RetentionTargetKind.Bundle);
        var window = new RetentionWindow(BaseTime, FutureTime);
        var reason = new RetentionReason("Historic hold.");

        var history = new object[]
        {
            new RetentionAppliedEvent(id, targetRef, window, reason, BaseTime)
        };

        var aggregate = (RetentionAggregate)Activator.CreateInstance(typeof(RetentionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.RetentionId);
        Assert.Equal(RetentionStatus.Applied, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
