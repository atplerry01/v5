using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed class ProviderAvailabilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderAvailabilityId Id { get; private set; }
    public ProviderRef Provider { get; private set; }
    public TimeWindow Window { get; private set; }
    public ProviderAvailabilityStatus Status { get; private set; }
    public int Version { get; private set; }

    private ProviderAvailabilityAggregate() { }

    public static ProviderAvailabilityAggregate Create(
        ProviderAvailabilityId id,
        ProviderRef provider,
        TimeWindow window)
    {
        var aggregate = new ProviderAvailabilityAggregate();

        var @event = new ProviderAvailabilityCreatedEvent(id, provider, window);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void UpdateWindow(TimeWindow window)
    {
        EnsureMutable();

        var @event = new ProviderAvailabilityUpdatedEvent(Id, window);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderAvailabilityActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProviderAvailabilityStatus.Archived)
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProviderAvailabilityArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderAvailabilityCreatedEvent @event)
    {
        Id = @event.ProviderAvailabilityId;
        Provider = @event.Provider;
        Window = @event.Window;
        Status = ProviderAvailabilityStatus.Draft;
        Version++;
    }

    private void Apply(ProviderAvailabilityUpdatedEvent @event)
    {
        Window = @event.Window;
        Version++;
    }

    private void Apply(ProviderAvailabilityActivatedEvent @event)
    {
        Status = ProviderAvailabilityStatus.Active;
        Version++;
    }

    private void Apply(ProviderAvailabilityArchivedEvent @event)
    {
        Status = ProviderAvailabilityStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderAvailabilityErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderAvailabilityErrors.MissingId();

        if (Provider == default)
            throw ProviderAvailabilityErrors.MissingProviderRef();

        if (!Enum.IsDefined(Status))
            throw ProviderAvailabilityErrors.InvalidStateTransition(Status, "validate");
    }
}
