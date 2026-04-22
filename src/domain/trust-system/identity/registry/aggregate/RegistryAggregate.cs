using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public sealed class RegistryAggregate : AggregateRoot
{
    public RegistryId RegistryId { get; private set; }
    public RegistrationDescriptor Descriptor { get; private set; }
    public RegistrationStatus Status { get; private set; }

    private RegistryAggregate() { }

    public static RegistryAggregate Initiate(RegistryId id, RegistrationDescriptor descriptor, Timestamp initiatedAt)
    {
        var aggregate = new RegistryAggregate();
        aggregate.RaiseDomainEvent(new RegistrationInitiatedEvent(id, descriptor, initiatedAt));
        return aggregate;
    }

    public void Verify()
    {
        if (Status != RegistrationStatus.Initiated)
            throw new DomainInvariantViolationException("Registration can only be verified from Initiated status.");

        RaiseDomainEvent(new RegistrationVerifiedEvent(RegistryId));
    }

    public void Activate()
    {
        if (Status != RegistrationStatus.Verified)
            throw new DomainInvariantViolationException("Registration can only be activated from Verified status.");

        RaiseDomainEvent(new RegistrationActivatedEvent(RegistryId));
    }

    public void Reject(string reason)
    {
        if (Status == RegistrationStatus.Activated || Status == RegistrationStatus.Rejected)
            throw new DomainInvariantViolationException("Registration cannot be rejected from its current status.");

        Guard.Against(string.IsNullOrWhiteSpace(reason), "Rejection reason must not be empty.");

        RaiseDomainEvent(new RegistrationRejectedEvent(RegistryId, reason.Trim()));
    }

    public void LockOut(string reason)
    {
        if (Status == RegistrationStatus.Activated || Status == RegistrationStatus.Locked)
            throw new DomainInvariantViolationException("Registration cannot be locked from its current status.");

        Guard.Against(string.IsNullOrWhiteSpace(reason), "Lock reason must not be empty.");

        RaiseDomainEvent(new RegistrationLockedEvent(RegistryId, reason.Trim()));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case RegistrationInitiatedEvent e:
                RegistryId = e.RegistryId;
                Descriptor = e.Descriptor;
                Status = RegistrationStatus.Initiated;
                break;

            case RegistrationVerifiedEvent:
                Status = RegistrationStatus.Verified;
                break;

            case RegistrationActivatedEvent:
                Status = RegistrationStatus.Activated;
                break;

            case RegistrationRejectedEvent:
                Status = RegistrationStatus.Rejected;
                break;

            case RegistrationLockedEvent:
                Status = RegistrationStatus.Locked;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(RegistryId == default, "Registry identity must be established.");
        Guard.Against(Descriptor == default, "Registration descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Registration status is not a defined enum value.");
    }
}
