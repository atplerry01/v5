using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed class ProviderCapabilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderCapabilityId Id { get; private set; }
    public ProviderRef Provider { get; private set; }
    public CapabilityCode Code { get; private set; }
    public CapabilityName Name { get; private set; }
    public CapabilityStatus Status { get; private set; }
    public int Version { get; private set; }

    private ProviderCapabilityAggregate() { }

    public static ProviderCapabilityAggregate Create(
        ProviderCapabilityId id,
        ProviderRef provider,
        CapabilityCode code,
        CapabilityName name)
    {
        var aggregate = new ProviderCapabilityAggregate();

        var @event = new ProviderCapabilityCreatedEvent(id, provider, code, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(CapabilityName name)
    {
        EnsureMutable();

        var @event = new ProviderCapabilityUpdatedEvent(Id, name);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderCapabilityActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == CapabilityStatus.Archived)
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProviderCapabilityArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderCapabilityCreatedEvent @event)
    {
        Id = @event.ProviderCapabilityId;
        Provider = @event.Provider;
        Code = @event.Code;
        Name = @event.Name;
        Status = CapabilityStatus.Draft;
        Version++;
    }

    private void Apply(ProviderCapabilityUpdatedEvent @event)
    {
        Name = @event.Name;
        Version++;
    }

    private void Apply(ProviderCapabilityActivatedEvent @event)
    {
        Status = CapabilityStatus.Active;
        Version++;
    }

    private void Apply(ProviderCapabilityArchivedEvent @event)
    {
        Status = CapabilityStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderCapabilityErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderCapabilityErrors.MissingId();

        if (Provider == default)
            throw ProviderCapabilityErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderCapabilityErrors.InvalidStateTransition(Status, "validate");
    }
}
