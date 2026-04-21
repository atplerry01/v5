using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed class ProfileAggregate : AggregateRoot
{
    private readonly Dictionary<string, ProfileDescriptor> _descriptors = new();

    public ProfileId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public ProfileDisplayName DisplayName { get; private set; }
    public ProfileStatus Status { get; private set; }
    public IReadOnlyDictionary<string, ProfileDescriptor> Descriptors => _descriptors;

    public static ProfileAggregate Create(
        ProfileId id,
        CustomerRef customer,
        ProfileDisplayName displayName)
    {
        var aggregate = new ProfileAggregate();
        if (aggregate.Version >= 0)
            throw ProfileErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProfileCreatedEvent(id, customer, displayName));
        return aggregate;
    }

    public void Rename(ProfileDisplayName displayName)
    {
        EnsureMutable();
        RaiseDomainEvent(new ProfileRenamedEvent(Id, displayName));
    }

    public void SetDescriptor(ProfileDescriptor descriptor)
    {
        EnsureMutable();
        RaiseDomainEvent(new ProfileDescriptorSetEvent(Id, descriptor));
    }

    public void RemoveDescriptor(string key)
    {
        EnsureMutable();

        if (!_descriptors.ContainsKey(key))
            throw ProfileErrors.DescriptorNotPresent(key);

        RaiseDomainEvent(new ProfileDescriptorRemovedEvent(Id, key));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProfileErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProfileActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ProfileStatus.Archived)
            throw ProfileErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProfileArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProfileCreatedEvent e:
                Id = e.ProfileId;
                Customer = e.Customer;
                DisplayName = e.DisplayName;
                Status = ProfileStatus.Draft;
                break;
            case ProfileRenamedEvent e:
                DisplayName = e.DisplayName;
                break;
            case ProfileDescriptorSetEvent e:
                _descriptors[e.Descriptor.Key] = e.Descriptor;
                break;
            case ProfileDescriptorRemovedEvent e:
                _descriptors.Remove(e.Key);
                break;
            case ProfileActivatedEvent:
                Status = ProfileStatus.Active;
                break;
            case ProfileArchivedEvent:
                Status = ProfileStatus.Archived;
                break;
        }
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProfileErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProfileErrors.MissingId();

        if (Customer == default)
            throw ProfileErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw ProfileErrors.InvalidStateTransition(Status, "validate");
    }
}
