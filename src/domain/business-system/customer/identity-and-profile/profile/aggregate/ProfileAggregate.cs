using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public sealed class ProfileAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly Dictionary<string, ProfileDescriptor> _descriptors = new();

    public ProfileId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public ProfileDisplayName DisplayName { get; private set; }
    public ProfileStatus Status { get; private set; }
    public IReadOnlyDictionary<string, ProfileDescriptor> Descriptors => _descriptors;
    public int Version { get; private set; }

    private ProfileAggregate() { }

    public static ProfileAggregate Create(
        ProfileId id,
        CustomerRef customer,
        ProfileDisplayName displayName)
    {
        var aggregate = new ProfileAggregate();

        var @event = new ProfileCreatedEvent(id, customer, displayName);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Rename(ProfileDisplayName displayName)
    {
        EnsureMutable();

        var @event = new ProfileRenamedEvent(Id, displayName);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void SetDescriptor(ProfileDescriptor descriptor)
    {
        EnsureMutable();

        var @event = new ProfileDescriptorSetEvent(Id, descriptor);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RemoveDescriptor(string key)
    {
        EnsureMutable();

        if (!_descriptors.ContainsKey(key))
            throw ProfileErrors.DescriptorNotPresent(key);

        var @event = new ProfileDescriptorRemovedEvent(Id, key);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProfileErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProfileActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProfileStatus.Archived)
            throw ProfileErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProfileArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProfileCreatedEvent @event)
    {
        Id = @event.ProfileId;
        Customer = @event.Customer;
        DisplayName = @event.DisplayName;
        Status = ProfileStatus.Draft;
        Version++;
    }

    private void Apply(ProfileRenamedEvent @event)
    {
        DisplayName = @event.DisplayName;
        Version++;
    }

    private void Apply(ProfileDescriptorSetEvent @event)
    {
        _descriptors[@event.Descriptor.Key] = @event.Descriptor;
        Version++;
    }

    private void Apply(ProfileDescriptorRemovedEvent @event)
    {
        _descriptors.Remove(@event.Key);
        Version++;
    }

    private void Apply(ProfileActivatedEvent @event)
    {
        Status = ProfileStatus.Active;
        Version++;
    }

    private void Apply(ProfileArchivedEvent @event)
    {
        Status = ProfileStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProfileErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProfileErrors.MissingId();

        if (Customer == default)
            throw ProfileErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Status))
            throw ProfileErrors.InvalidStateTransition(Status, "validate");
    }
}
