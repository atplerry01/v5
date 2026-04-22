using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Access.Grant;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Access.Grant;

public sealed class GrantAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static GrantId NewId(string seed) =>
        new(IdGen.Generate($"GrantAggregateTests:{seed}:grant"));

    private static GrantDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("GrantAggregateTests:principal-ref"), "content:read", "ResourceAccess");

    [Fact]
    public void Issue_RaisesGrantIssuedEvent()
    {
        var id = NewId("Issue_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = GrantAggregate.Issue(id, descriptor, FixedTs);
        var evt = Assert.IsType<GrantIssuedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.GrantId);
        Assert.Equal(descriptor.GrantScope, evt.Descriptor.GrantScope);
    }

    [Fact]
    public void Issue_SetsStatusToIssued()
    {
        var aggregate = GrantAggregate.Issue(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(GrantStatus.Issued, aggregate.Status);
    }

    [Fact]
    public void Activate_FromIssued_SetsStatusToActive()
    {
        var aggregate = GrantAggregate.Issue(NewId("Activate"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Activate();
        Assert.IsType<GrantActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(GrantStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromActive_SetsStatusToRevoked()
    {
        var aggregate = GrantAggregate.Issue(NewId("Revoke_Active"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Revoke();
        Assert.Equal(GrantStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromIssued_SetsStatusToRevoked()
    {
        var aggregate = GrantAggregate.Issue(NewId("Revoke_Issued"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Revoke();
        Assert.Equal(GrantStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Expire_FromActive_SetsStatusToExpired()
    {
        var aggregate = GrantAggregate.Issue(NewId("Expire"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Expire();
        Assert.Equal(GrantStatus.Expired, aggregate.Status);
    }

    [Fact]
    public void Revoke_AfterRevoke_Throws()
    {
        var aggregate = GrantAggregate.Issue(NewId("Revoke_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.Revoke();
        Assert.ThrowsAny<Exception>(() => aggregate.Revoke());
    }

    [Fact]
    public void Expire_AfterExpire_Throws()
    {
        var aggregate = GrantAggregate.Issue(NewId("Expire_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.Expire();
        Assert.ThrowsAny<Exception>(() => aggregate.Expire());
    }

    [Fact]
    public void Activate_WhenAlreadyActive_Throws()
    {
        var aggregate = GrantAggregate.Issue(NewId("Activate_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new GrantIssuedEvent(id, descriptor, FixedTs), new GrantActivatedEvent(id), new GrantRevokedEvent(id) };
        var aggregate = (GrantAggregate)Activator.CreateInstance(typeof(GrantAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.GrantId);
        Assert.Equal(GrantStatus.Revoked, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
