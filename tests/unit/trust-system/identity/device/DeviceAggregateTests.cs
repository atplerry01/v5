using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Device;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.Identity.Device;

public sealed class DeviceAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FixedTs = new(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

    private static DeviceId NewId(string seed) =>
        new(IdGen.Generate($"DeviceAggregateTests:{seed}:device"));

    private static DeviceDescriptor DefaultDescriptor() =>
        new(IdGen.Generate("DeviceAggregateTests:identity-ref"), "Alice-iPhone", "Mobile");

    [Fact]
    public void Register_RaisesDeviceRegisteredEvent()
    {
        var id = NewId("Register_Valid");
        var descriptor = DefaultDescriptor();
        var aggregate = DeviceAggregate.Register(id, descriptor, FixedTs);
        var evt = Assert.IsType<DeviceRegisteredEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.DeviceId);
        Assert.Equal(descriptor.DeviceName, evt.Descriptor.DeviceName);
    }

    [Fact]
    public void Register_SetsStatusToRegistered()
    {
        var aggregate = DeviceAggregate.Register(NewId("Status"), DefaultDescriptor(), FixedTs);
        Assert.Equal(DeviceStatus.Registered, aggregate.Status);
    }

    [Fact]
    public void Activate_FromRegistered_SetsStatusToActive()
    {
        var aggregate = DeviceAggregate.Register(NewId("Activate"), DefaultDescriptor(), FixedTs);
        aggregate.ClearDomainEvents();
        aggregate.Activate();
        Assert.IsType<DeviceActivatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(DeviceStatus.Active, aggregate.Status);
    }

    [Fact]
    public void Suspend_FromActive_SetsStatusToSuspended()
    {
        var aggregate = DeviceAggregate.Register(NewId("Suspend"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Suspend();
        Assert.Equal(DeviceStatus.Suspended, aggregate.Status);
    }

    [Fact]
    public void Deregister_FromActive_SetsStatusToDeregistered()
    {
        var aggregate = DeviceAggregate.Register(NewId("Deregister_Active"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.ClearDomainEvents();
        aggregate.Deregister();
        Assert.Equal(DeviceStatus.Deregistered, aggregate.Status);
    }

    [Fact]
    public void Activate_FromActive_Throws()
    {
        var aggregate = DeviceAggregate.Register(NewId("Activate_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        Assert.ThrowsAny<Exception>(() => aggregate.Activate());
    }

    [Fact]
    public void Suspend_FromRegistered_Throws()
    {
        var aggregate = DeviceAggregate.Register(NewId("Suspend_Registered"), DefaultDescriptor(), FixedTs);
        Assert.ThrowsAny<Exception>(() => aggregate.Suspend());
    }

    [Fact]
    public void Deregister_WhenAlreadyDeregistered_Throws()
    {
        var aggregate = DeviceAggregate.Register(NewId("Deregister_Again"), DefaultDescriptor(), FixedTs);
        aggregate.Activate();
        aggregate.Deregister();
        Assert.ThrowsAny<Exception>(() => aggregate.Deregister());
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var descriptor = DefaultDescriptor();
        var history = new object[] { new DeviceRegisteredEvent(id, descriptor, FixedTs), new DeviceActivatedEvent(id) };
        var aggregate = (DeviceAggregate)Activator.CreateInstance(typeof(DeviceAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);
        Assert.Equal(id, aggregate.DeviceId);
        Assert.Equal(DeviceStatus.Active, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
