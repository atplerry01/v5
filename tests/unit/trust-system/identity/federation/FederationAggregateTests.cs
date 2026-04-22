using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Federation;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Federation;

public sealed class FederationAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static FederationId NewId(string seed) =>
        new(IdGen.Generate($"FederationAggregateTests:{seed}:federation"));

    private static FederationDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("FederationAggregateTests:identity-ref"), "google-workspace", "OIDC");

    [Fact]
    public void Establish_RaisesFederationEstablishedEvent()
    {
        var id = NewId("Establish_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = FederationAggregate.Establish(id, descriptor, FixedTs);
        var evt = Assert.IsType<FederationEstablishedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.FederationId);
        Assert.Equal(descriptor.FederatedProvider, evt.Descriptor.FederatedProvider);
    }

    [Fact]
    public void Establish_SetsStatusToActive()
    {
        var aggregate = FederationAggregate.Establish(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(FederationStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var aggregate = FederationAggregate.Establish(NewId("Suspend"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Suspend();
        Assert.Equal(FederationStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Terminate_FromActive_SetsStatusToTerminated()
    {
        var aggregate = FederationAggregate.Establish(NewId("Terminate_Active"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Terminate();
        Assert.Equal(FederationStatus.Terminated, aggregate.Status);
    }

    [Fact]
    public void Terminate_FromSuspended_SetsStatusToTerminated()
    {
        var aggregate = FederationAggregate.Establish(NewId("Terminate_Suspended"), DefaultDescriptor(), FixedTs);
        aggregate.Suspend();
        aggregate.ClearDomainEvents();
        aggregate.Terminate();
        Assert.Equal(FederationStatus.Terminated, aggregate.Status);
    }

    [Fact]
    public void Terminate_WhenAlreadyTerminated_Throws()
    {
        var aggregate = FederationAggregate.Establish(NewId("Terminate_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Terminate();
        Assert.ThrowsAny<Exception>(() => aggregate.Terminate());
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_Throws()
    {
        var aggregate = FederationAggregate.Establish(NewId("Suspend_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Suspend();
        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new FederationEstablishedEvent(id, descriptor, FixedTs), new FederationSuspendedEvent(id) };
        var aggregate = (FederationAggregate)Activator.CreateInstance(typeof(FederationAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.FederationId);
        Assert.Equal(FederationStatus.Suspended, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
