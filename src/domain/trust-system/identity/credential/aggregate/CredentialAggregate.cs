namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed class CredentialAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CredentialId Id { get; private set; }
    public CredentialDescriptor Descriptor { get; private set; }
    public CredentialStatus Status { get; private set; }
    public int Version { get; private set; }

    private CredentialAggregate() { }

    public static CredentialAggregate Issue(CredentialId id, CredentialDescriptor descriptor)
    {
        if (id == default)
            throw CredentialErrors.MissingId();
        if (descriptor == default)
            throw CredentialErrors.MissingDescriptor();

        var aggregate = new CredentialAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CredentialIssuedEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Activate));

        ValidateBeforeChange();

        var @event = new CredentialActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);

        EnsureInvariants();
    }

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Revoke));

        ValidateBeforeChange();

        var @event = new CredentialRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);

        EnsureInvariants();
    }

    private void Apply(CredentialIssuedEvent @event)
    {
        Id = @event.CredentialId;
        Descriptor = @event.Descriptor;
        Status = CredentialStatus.Issued;
        Version++;
    }

    private void Apply(CredentialActivatedEvent @event)
    {
        Status = CredentialStatus.Active;
        Version++;
    }

    private void Apply(CredentialRevokedEvent @event)
    {
        Status = CredentialStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw CredentialErrors.MissingId();

        if (Descriptor == default)
            throw CredentialErrors.MissingDescriptor();

        if (!Enum.IsDefined(Status))
            throw new InvalidOperationException("CredentialStatus is not a defined enum value.");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
