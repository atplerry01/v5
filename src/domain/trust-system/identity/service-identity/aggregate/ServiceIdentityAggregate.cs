using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceIdentityAggregate : AggregateRoot
{
    public ServiceIdentityId ServiceIdentityId { get; private set; }
    public ServiceIdentityDescriptor Descriptor { get; private set; }
    public ServiceIdentityStatus Status { get; private set; }

    private ServiceIdentityAggregate() { }

    public static ServiceIdentityAggregate Register(ServiceIdentityId id, ServiceIdentityDescriptor descriptor, Timestamp registeredAt)
    {
        var aggregate = new ServiceIdentityAggregate();
        aggregate.RaiseDomainEvent(new ServiceIdentityRegisteredEvent(id, descriptor, registeredAt));
        return aggregate;
    }

    public void Suspend()
    {
        if (Status != ServiceIdentityStatus.Active)
            throw new DomainInvariantViolationException("Service identity can only be suspended from Active status.");
        RaiseDomainEvent(new ServiceIdentitySuspendedEvent(ServiceIdentityId));
    }

    public void Decommission()
    {
        if (Status == ServiceIdentityStatus.Decommissioned)
            throw new DomainInvariantViolationException("Service identity is already decommissioned.");
        RaiseDomainEvent(new ServiceIdentityDecommissionedEvent(ServiceIdentityId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ServiceIdentityRegisteredEvent e:
                ServiceIdentityId = e.ServiceIdentityId;
                Descriptor = e.Descriptor;
                Status = ServiceIdentityStatus.Active;
                break;
            case ServiceIdentitySuspendedEvent:
                Status = ServiceIdentityStatus.Suspended;
                break;
            case ServiceIdentityDecommissionedEvent:
                Status = ServiceIdentityStatus.Decommissioned;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(ServiceIdentityId == default, "Service identity must be established.");
        Guard.Against(Descriptor == default, "Service identity descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Service identity status is not a defined enum value.");
    }
}
