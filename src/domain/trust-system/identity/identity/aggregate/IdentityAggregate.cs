namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public IdentityId Id { get; private set; }
    public IdentityDescriptor Descriptor { get; private set; }
    public IdentityStatus Status { get; private set; }
    public int Version { get; private set; }

    private IdentityAggregate() { }

    public static IdentityAggregate Establish(IdentityId id, IdentityDescriptor descriptor)
    {
        var aggregate = new IdentityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new IdentityEstablishedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw IdentityErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new IdentitySuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw IdentityErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new IdentityTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(IdentityEstablishedEvent @event)
    {
        Id = @event.IdentityId;
        Descriptor = @event.Descriptor;
        Status = IdentityStatus.Active;
        Version++;
    }

    private void Apply(IdentitySuspendedEvent @event)
    {
        Status = IdentityStatus.Suspended;
        Version++;
    }

    private void Apply(IdentityTerminatedEvent @event)
    {
        Status = IdentityStatus.Terminated;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw IdentityErrors.MissingId();

        if (Descriptor == default)
            throw IdentityErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw IdentityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
