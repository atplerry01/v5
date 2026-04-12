namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AuthorityId Id { get; private set; }
    public AuthorityDescriptor Descriptor { get; private set; }
    public AuthorityStatus Status { get; private set; }
    public int Version { get; private set; }

    private AuthorityAggregate() { }

    public static AuthorityAggregate Establish(AuthorityId id, AuthorityDescriptor descriptor)
    {
        var aggregate = new AuthorityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AuthorityEstablishedEvent(id, descriptor);
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
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new AuthorityActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke()
    {
        ValidateBeforeChange();

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new AuthorityRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AuthorityEstablishedEvent @event)
    {
        Id = @event.AuthorityId;
        Descriptor = @event.Descriptor;
        Status = AuthorityStatus.Established;
        Version++;
    }

    private void Apply(AuthorityActivatedEvent @event)
    {
        Status = AuthorityStatus.Active;
        Version++;
    }

    private void Apply(AuthorityRevokedEvent @event)
    {
        Status = AuthorityStatus.Revoked;
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
            throw AuthorityErrors.MissingId();

        if (Descriptor == default)
            throw AuthorityErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw AuthorityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}