using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed class StreamAccessAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp FutureTime = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    private static StreamAccessId NewId(string seed) =>
        new(IdGen.Generate($"StreamAccessAggregateTests:{seed}:access"));

    [Fact]
    public void Grant_RaisesStreamAccessGrantedEvent()
    {
        var id = NewId("Grant_Valid");
        var streamRef = new StreamRef(IdGen.Generate("StreamAccessAggregateTests:stream-ref"));
        var window = new AccessWindow(BaseTime, FutureTime);
        var token = new TokenBinding("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.valid");

        var aggregate = StreamAccessAggregate.Grant(id, streamRef, AccessMode.Read, window, token, BaseTime);

        var evt = Assert.IsType<StreamAccessGrantedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AccessId);
        Assert.Equal(streamRef, evt.StreamRef);
        Assert.Equal(AccessMode.Read, evt.Mode);
    }

    [Fact]
    public void Grant_SetsStateFromEvent()
    {
        var id = NewId("Grant_State");
        var streamRef = new StreamRef(IdGen.Generate("StreamAccessAggregateTests:stream-state"));
        var window = new AccessWindow(BaseTime, FutureTime);
        var token = new TokenBinding("token-abc");

        var aggregate = StreamAccessAggregate.Grant(id, streamRef, AccessMode.Publish, window, token, BaseTime);

        Assert.Equal(id, aggregate.AccessId);
        Assert.Equal(AccessStatus.Granted, aggregate.Status);
    }

    [Fact]
    public void Grant_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var streamRef = new StreamRef(IdGen.Generate("StreamAccessAggregateTests:stable-stream"));
        var window = new AccessWindow(BaseTime, FutureTime);
        var token = new TokenBinding("stable-token");
        var a1 = StreamAccessAggregate.Grant(id, streamRef, AccessMode.Read, window, token, BaseTime);
        var a2 = StreamAccessAggregate.Grant(id, streamRef, AccessMode.Read, window, token, BaseTime);

        Assert.Equal(
            ((StreamAccessGrantedEvent)a1.DomainEvents[0]).AccessId.Value,
            ((StreamAccessGrantedEvent)a2.DomainEvents[0]).AccessId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesStreamAccessState()
    {
        var id = NewId("History");
        var streamRef = new StreamRef(IdGen.Generate("StreamAccessAggregateTests:history-stream"));
        var window = new AccessWindow(BaseTime, FutureTime);
        var token = new TokenBinding("history-token");

        var history = new object[]
        {
            new StreamAccessGrantedEvent(id, streamRef, AccessMode.Read, window, token, BaseTime)
        };

        var aggregate = (StreamAccessAggregate)Activator.CreateInstance(typeof(StreamAccessAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.AccessId);
        Assert.Equal(AccessStatus.Granted, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
