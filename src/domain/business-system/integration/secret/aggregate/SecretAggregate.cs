namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public sealed class SecretAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SecretId Id { get; private set; }
    public SecretDescriptor Descriptor { get; private set; }
    public SecretStatus Status { get; private set; }
    public int Version { get; private set; }

    private SecretAggregate() { }

    public static SecretAggregate Store(SecretId id, SecretDescriptor descriptor)
    {
        var aggregate = new SecretAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SecretStoredEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SecretErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SecretActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Rotate()
    {
        ValidateBeforeChange();

        var specification = new CanRotateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SecretErrors.InvalidStateTransition(Status, nameof(Rotate));

        var @event = new SecretRotatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Retire()
    {
        ValidateBeforeChange();

        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SecretErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new SecretRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SecretStoredEvent @event)
    {
        Id = @event.SecretId;
        Descriptor = @event.Descriptor;
        Status = SecretStatus.Stored;
        Version++;
    }

    private void Apply(SecretActivatedEvent @event)
    {
        Status = SecretStatus.Active;
        Version++;
    }

    private void Apply(SecretRotatedEvent @event)
    {
        Status = SecretStatus.Rotated;
        Version++;
    }

    private void Apply(SecretRetiredEvent @event)
    {
        Status = SecretStatus.Retired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SecretErrors.MissingId();

        if (Descriptor == default)
            throw SecretErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw SecretErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
