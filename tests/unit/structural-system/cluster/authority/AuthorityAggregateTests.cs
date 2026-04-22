using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Cluster.Authority;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.StructuralSystem.Cluster.Authority;

public sealed class AuthorityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static AuthorityId NewId(string seed) =>
        new(IdGen.Generate($"AuthorityAggregateTests:{seed}:authority"));

    private static AuthorityDescriptor DefaultDescriptor() =>
        new(new ClusterRef(IdGen.Generate("AuthorityAggregateTests:cluster-ref")), "Alpha Authority");

    [Fact]
    public void Establish_RaisesAuthorityEstablishedEvent()
    {
        var id = NewId("Establish_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = AuthorityAggregate.Establish(id, descriptor);

        var evt = Assert.IsType<AuthorityEstablishedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.AuthorityId);
        Assert.Equal(descriptor.AuthorityName, evt.Descriptor.AuthorityName);
    }

    [Fact]
    public void Establish_SetsStatusToEstablished()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Establish_Status"), DefaultDescriptor());

        Assert.Equal(AuthorityStatus.Established, aggregate.Status);
    }

    [Fact]
    public void Activate_FromEstablished_RaisesAuthorityActivatedEvent()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Activate_Valid"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Activate();

        Assert.IsType<AuthorityActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuthorityStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Revoke_FromActive_RaisesAuthorityRevokedEvent()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Revoke_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Revoke();

        Assert.IsType<AuthorityRevokedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuthorityStatus.Revoked, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_RaisesAuthoritySuspendedEvent()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Suspend_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<AuthoritySuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuthorityStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Retire_FromActive_RaisesAuthorityRetiredEvent()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Retire_Valid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.ClearDomainEvents();

        aggregate.Retire();

        Assert.IsType<AuthorityRetiredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(AuthorityStatus.Retired, aggregate.Status);
    }

    [Fact]
    public void Activate_FromRevoked_Throws()
    {
        var aggregate = AuthorityAggregate.Establish(NewId("Activate_Invalid"), DefaultDescriptor());
        aggregate.Activate();
        aggregate.Revoke();

        Assert.ThrowsAny<DomainException>(() => aggregate.Activate());
    }

    [Fact]
    public void Establish_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var a1 = AuthorityAggregate.Establish(id, DefaultDescriptor());
        var a2 = AuthorityAggregate.Establish(id, DefaultDescriptor());

        Assert.Equal(
            ((AuthorityEstablishedEvent)a1.DomainEvents[0]).AuthorityId.Value,
            ((AuthorityEstablishedEvent)a2.DomainEvents[0]).AuthorityId.Value);
    }

    [Fact]
    public void LoadFromHistory_RehydratesAuthorityState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new AuthorityEstablishedEvent(id, descriptor),
            new AuthorityActivatedEvent(id)
        };

        var aggregate = (AuthorityAggregate)Activator.CreateInstance(typeof(AuthorityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(AuthorityStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
