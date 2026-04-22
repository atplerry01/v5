using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Access.Session;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Session;

public sealed class SessionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static SessionId NewId(string seed) =>
        new(IdGen.Generate($"SessionAggregateTests:{seed}:session"));

    private static SessionDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("SessionAggregateTests:identity-ref"), "web-dashboard");

    [Fact]
    public void Open_RaisesSessionOpenedEvent()
    {
        var id = NewId("Open_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = SessionAggregate.Open(id, descriptor, FixedTs);
        var evt = Assert.IsType<SessionOpenedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.SessionId);
        Assert.Equal(descriptor.SessionContext, evt.Descriptor.SessionContext);
    }

    [Fact]
    public void Open_SetsStatusToActive()
    {
        var aggregate = SessionAggregate.Open(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(SessionStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Expire_FromActive_SetsStatusToExpired()
    {
        var aggregate = SessionAggregate.Open(NewId("Expire"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Expire();
        Assert.IsType<SessionExpiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SessionStatus.Expired, aggregate.Status);
    }

    [Fact]
    public void Terminate_FromActive_SetsStatusToTerminated()
    {
        var aggregate = SessionAggregate.Open(NewId("Terminate"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Terminate();
        Assert.IsType<SessionTerminatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(SessionStatus.Terminated, aggregate.Status);
    }

    [Fact]
    public void Expire_AfterExpired_Throws()
    {
        var aggregate = SessionAggregate.Open(NewId("Expire_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Expire();
        Assert.ThrowsAny<Exception>(() => aggregate.Expire());
    }

    [Fact]
    public void Terminate_AfterExpired_Throws()
    {
        var aggregate = SessionAggregate.Open(NewId("Terminate_Expired"), DefaultDescriptor(), FixedTs);
        aggregate.Expire();
        Assert.ThrowsAny<Exception>(() => aggregate.Terminate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new SessionOpenedEvent(id, descriptor, FixedTs), new SessionTerminatedEvent(id) };
        var aggregate = (SessionAggregate)Activator.CreateInstance(typeof(SessionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.SessionId);
        Assert.Equal(SessionStatus.Terminated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
