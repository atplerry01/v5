using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Device;

public sealed class DeviceAggregate : AggregateRoot
{
    public DeviceId DeviceId { get; private set; }
    public DeviceDescriptor Descriptor { get; private set; }
    public DeviceStatus Status { get; private set; }

    private DeviceAggregate() { }

    public static DeviceAggregate Register(DeviceId id, DeviceDescriptor descriptor, Timestamp registeredAt)
    {
        var aggregate = new DeviceAggregate();
        aggregate.RaiseDomainEvent(new DeviceRegisteredEvent(id, descriptor, registeredAt));
        return aggregate;
    }

    public void Activate()
    {
        if (Status != DeviceStatus.Registered)
            throw new DomainInvariantViolationException("Device can only be activated from Registered status.");
        RaiseDomainEvent(new DeviceActivatedEvent(DeviceId));
    }

    public void Suspend()
    {
        if (Status != DeviceStatus.Active)
            throw new DomainInvariantViolationException("Device can only be suspended from Active status.");
        RaiseDomainEvent(new DeviceSuspendedEvent(DeviceId));
    }

    public void Deregister()
    {
        if (Status == DeviceStatus.Deregistered)
            throw new DomainInvariantViolationException("Device is already deregistered.");
        RaiseDomainEvent(new DeviceDeregisteredEvent(DeviceId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case DeviceRegisteredEvent e:
                DeviceId = e.DeviceId;
                Descriptor = e.Descriptor;
                Status = DeviceStatus.Registered;
                break;
            case DeviceActivatedEvent:
                Status = DeviceStatus.Active;
                break;
            case DeviceSuspendedEvent:
                Status = DeviceStatus.Suspended;
                break;
            case DeviceDeregisteredEvent:
                Status = DeviceStatus.Deregistered;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(DeviceId == default, "Device identity must be established.");
        Guard.Against(Descriptor == default, "Device descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Device status is not a defined enum value.");
    }
}
