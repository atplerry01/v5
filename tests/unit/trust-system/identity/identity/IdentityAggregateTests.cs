using Whycespace.Domain.TrustSystem.Identity.Identity;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Identity;

public sealed class IdentityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static IdentityId NewId(string seed) =>
        new(IdGen.Generate($"IdentityAggregateTests:{seed}:identity"));

    private static IdentityDescriptor DefaultDescriptor() =>
        new("alice@whycespace.com", "Person");

    [Fact]
    public void Establish_RaisesIdentityEstablishedEvent()
    {
        var id = NewId("Establish_Valid");
        var descriptor = DefaultDescriptor();

        var aggregate = IdentityAggregate.Establish(id, descriptor);

        var evt = Assert.IsType<IdentityEstablishedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.IdentityId);
        Assert.Equal(descriptor.PrincipalName, evt.Descriptor.PrincipalName);
        Assert.Equal(descriptor.PrincipalType, evt.Descriptor.PrincipalType);
    }

    [Fact]
    public void Establish_SetsStatusToActive()
    {
        var aggregate = IdentityAggregate.Establish(NewId("State"), DefaultDescriptor());

        Assert.Equal(IdentityStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Establish_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var descriptor = DefaultDescriptor();

        var a1 = IdentityAggregate.Establish(id, descriptor);
        var a2 = IdentityAggregate.Establish(id, descriptor);

        Assert.Equal(
            ((IdentityEstablishedEvent)a1.DomainEvents[0]).IdentityId.Value,
            ((IdentityEstablishedEvent)a2.DomainEvents[0]).IdentityId.Value);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var aggregate = IdentityAggregate.Establish(NewId("Suspend"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Suspend();

        Assert.IsType<IdentitySuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IdentityStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Terminate_FromActive_SetsStatusToTerminated()
    {
        var aggregate = IdentityAggregate.Establish(NewId("Terminate"), DefaultDescriptor());
        aggregate.ClearDomainEvents();

        aggregate.Terminate();

        Assert.IsType<IdentityTerminatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(IdentityStatus.Terminated, aggregate.Status);
    }

    [Fact]
    public void Terminate_FromSuspended_SetsStatusToTerminated()
    {
        var aggregate = IdentityAggregate.Establish(NewId("Term_Suspended"), DefaultDescriptor());
        aggregate.Suspend();
        aggregate.ClearDomainEvents();

        aggregate.Terminate();

        Assert.Equal(IdentityStatus.Terminated, aggregate.Status);
    }

    [Fact]
    public void Suspend_AfterTerminate_Throws()
    {
        var aggregate = IdentityAggregate.Establish(NewId("Suspend_Terminated"), DefaultDescriptor());
        aggregate.Terminate();

        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void Terminate_AfterTerminate_Throws()
    {
        var aggregate = IdentityAggregate.Establish(NewId("Terminate_Again"), DefaultDescriptor());
        aggregate.Terminate();

        Assert.ThrowsAny<Exception>(() => aggregate.Terminate());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();

        var history = new object[]
        {
            new IdentityEstablishedEvent(id, descriptor)
        };

        var aggregate = (IdentityAggregate)Activator.CreateInstance(typeof(IdentityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(descriptor.PrincipalName, aggregate.Descriptor.PrincipalName);
        Assert.Equal(IdentityStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
