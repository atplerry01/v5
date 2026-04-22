using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed class ObservabilityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp EndTime = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));

    private static ObservabilityId NewId(string seed) =>
        new(IdGen.Generate($"ObservabilityAggregateTests:{seed}:observability"));

    private static ObservabilitySnapshot DefaultSnapshot() => new(
        new ViewerCount(100),
        new PlaybackCount(500),
        new ErrorCount(2),
        new DropCount(5),
        new BitrateMeasurement(4_000_000),
        new LatencyMeasurement(120));

    [Fact]
    public void Capture_RaisesObservabilityCapturedEvent()
    {
        var id = NewId("Capture_Valid");
        var streamRef = new StreamRef(IdGen.Generate("ObservabilityAggregateTests:stream-ref"));
        var window = new ObservabilityWindow(BaseTime, EndTime);
        var snapshot = DefaultSnapshot();

        var aggregate = ObservabilityAggregate.Capture(id, streamRef, null, window, snapshot, BaseTime);

        var evt = Assert.IsType<ObservabilityCapturedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ObservabilityId);
        Assert.Equal(streamRef, evt.StreamRef);
    }

    [Fact]
    public void Capture_SetsStateFromEvent()
    {
        var id = NewId("Capture_State");
        var streamRef = new StreamRef(IdGen.Generate("ObservabilityAggregateTests:stream-state"));
        var window = new ObservabilityWindow(BaseTime, EndTime);

        var aggregate = ObservabilityAggregate.Capture(id, streamRef, null, window, DefaultSnapshot(), BaseTime);

        Assert.Equal(id, aggregate.ObservabilityId);
    }

    [Fact]
    public void Capture_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = new StreamRef(IdGen.Generate("ObservabilityAggregateTests:stable-stream"));
        var window = new ObservabilityWindow(BaseTime, EndTime);
        var o1 = ObservabilityAggregate.Capture(id, streamRef, null, window, DefaultSnapshot(), BaseTime);
        var o2 = ObservabilityAggregate.Capture(id, streamRef, null, window, DefaultSnapshot(), BaseTime);

        Assert.Equal(
            ((ObservabilityCapturedEvent)o1.DomainEvents[0]).ObservabilityId.Value,
            ((ObservabilityCapturedEvent)o2.DomainEvents[0]).ObservabilityId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesObservabilityState()
    {
        var id = NewId("History");
        var streamRef = new StreamRef(IdGen.Generate("ObservabilityAggregateTests:history-stream"));
        var window = new ObservabilityWindow(BaseTime, EndTime);

        var history = new object[]
        {
            new ObservabilityCapturedEvent(id, streamRef, null, window, DefaultSnapshot(), BaseTime)
        };

        var aggregate = (ObservabilityAggregate)Activator.CreateInstance(typeof(ObservabilityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.ObservabilityId);
        Assert.Empty(aggregate.DomainEvents);
    }
}
