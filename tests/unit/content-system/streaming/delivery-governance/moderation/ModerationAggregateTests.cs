using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.DeliveryGovernance.Moderation;

public sealed class ModerationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static ModerationId NewId(string seed) =>
        new(IdGen.Generate($"ModerationAggregateTests:{seed}:moderation"));

    [Fact]
    public void Flag_RaisesStreamFlaggedEvent()
    {
        var id = NewId("Flag_Valid");
        var targetRef = new ModerationTargetRef(IdGen.Generate("ModerationAggregateTests:target-ref"));
        const string reason = "Inappropriate content detected.";

        var aggregate = ModerationAggregate.Flag(id, targetRef, reason, BaseTime);

        var evt = Assert.IsType<StreamFlaggedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ModerationId);
        Assert.Equal(targetRef, evt.TargetRef);
        Assert.Equal(reason, evt.FlagReason);
    }

    [Fact]
    public void Flag_SetsStateFromEvent()
    {
        var id = NewId("Flag_State");
        var targetRef = new ModerationTargetRef(IdGen.Generate("ModerationAggregateTests:target-state"));

        var aggregate = ModerationAggregate.Flag(id, targetRef, "Test violation.", BaseTime);

        Assert.Equal(id, aggregate.ModerationId);
        Assert.Equal(ModerationStatus.Flagged, aggregate.Status);
    }

    [Fact]
    public void Flag_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var targetRef = new ModerationTargetRef(IdGen.Generate("ModerationAggregateTests:stable-target"));
        var m1 = ModerationAggregate.Flag(id, targetRef, "Stable flag.", BaseTime);
        var m2 = ModerationAggregate.Flag(id, targetRef, "Stable flag.", BaseTime);

        Assert.Equal(
            ((StreamFlaggedEvent)m1.DomainEvents[0]).ModerationId.Value,
            ((StreamFlaggedEvent)m2.DomainEvents[0]).ModerationId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesModerationState()
    {
        var id = NewId("History");
        var targetRef = new ModerationTargetRef(IdGen.Generate("ModerationAggregateTests:history-target"));

        var history = new object[]
        {
            new StreamFlaggedEvent(id, targetRef, "History flag.", BaseTime)
        };

        var aggregate = (ModerationAggregate)Activator.CreateInstance(typeof(ModerationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ModerationId);
        Assert.Equal(ModerationStatus.Flagged, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
