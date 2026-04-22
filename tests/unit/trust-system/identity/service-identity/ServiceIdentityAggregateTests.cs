using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceIdentityAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static ServiceIdentityId NewId(string seed) =>
        new(IdGen.Generate($"ServiceIdentityAggregateTests:{seed}:service-identity"));

    private static ServiceIdentityDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("ServiceIdentityAggregateTests:owner-ref"), "payment-gateway", "ExternalService");

    [Fact]
    public void Register_RaisesServiceIdentityRegisteredEvent()
    {
        var id = NewId("Register_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = ServiceIdentityAggregate.Register(id, descriptor, FixedTs);
        var evt = Assert.IsType<ServiceIdentityRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.ServiceIdentityId);
        Assert.Equal(descriptor.ServiceName, evt.Descriptor.ServiceName);
    }

    [Fact]
    public void Register_SetsStatusToActive()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(ServiceIdentityStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Suspend"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Suspend();
        Assert.IsType<ServiceIdentitySuspendedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ServiceIdentityStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Decommission_FromActive_SetsStatusToDecommissioned()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Decommission_Active"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Decommission();
        Assert.Equal(ServiceIdentityStatus.Decommissioned, aggregate.Status);
    }

    [Fact]
    public void Decommission_FromSuspended_SetsStatusToDecommissioned()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Decommission_Suspended"), DefaultDescriptor(), FixedTs);
        aggregate.Suspend();
        aggregate.ClearDomainEvents();
        aggregate.Decommission();
        Assert.Equal(ServiceIdentityStatus.Decommissioned, aggregate.Status);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_Throws()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Suspend_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Suspend();
        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void Decommission_WhenAlreadyDecommissioned_Throws()
    {
        var aggregate = ServiceIdentityAggregate.Register(NewId("Decommission_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Decommission();
        Assert.ThrowsAny<Exception>(() => aggregate.Decommission());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new ServiceIdentityRegisteredEvent(id, descriptor, FixedTs), new ServiceIdentitySuspendedEvent(id) };
        var aggregate = (ServiceIdentityAggregate)Activator.CreateInstance(typeof(ServiceIdentityAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.ServiceIdentityId);
        Assert.Equal(ServiceIdentityStatus.Suspended, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
