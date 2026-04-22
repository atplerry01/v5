using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class ProfileAggregate : AggregateRoot
{
    public ProfileId ProfileId { get; private set; }
    public ProfileDescriptor Descriptor { get; private set; }
    public ProfileStatus Status { get; private set; }

    private ProfileAggregate() { }

    public static ProfileAggregate Create(ProfileId id, ProfileDescriptor descriptor, Timestamp createdAt)
    {
        var aggregate = new ProfileAggregate();
        aggregate.RaiseDomainEvent(new ProfileCreatedEvent(id, descriptor, createdAt));
        return aggregate;
    }

    public void Activate()
    {
        if (Status == ProfileStatus.Deactivated)
            throw new DomainInvariantViolationException("Cannot activate a deactivated profile.");

        RaiseDomainEvent(new ProfileActivatedEvent(ProfileId));
    }

    public void Deactivate()
    {
        if (Status == ProfileStatus.Deactivated)
            throw new DomainInvariantViolationException("Profile is already deactivated.");

        RaiseDomainEvent(new ProfileDeactivatedEvent(ProfileId));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProfileCreatedEvent e:
                ProfileId = e.ProfileId;
                Descriptor = e.Descriptor;
                Status = ProfileStatus.Created;
                break;

            case ProfileActivatedEvent:
                Status = ProfileStatus.Active;
                break;

            case ProfileDeactivatedEvent:
                Status = ProfileStatus.Deactivated;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(ProfileId == default, "Profile identity must be established.");
        Guard.Against(Descriptor == default, "Profile descriptor must be present.");
        Guard.Against(!Enum.IsDefined(Status), "Profile status is not a defined enum value.");
    }
}
