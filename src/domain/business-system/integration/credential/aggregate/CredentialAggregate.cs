namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public sealed class CredentialAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CredentialId Id { get; private set; }
    public CredentialDescriptor Descriptor { get; private set; }
    public CredentialStatus Status { get; private set; }
    public int Version { get; private set; }

    private CredentialAggregate() { }

    // -- Factory ----------------------------------------------------------

    public static CredentialAggregate Register(
        CredentialId id,
        CredentialDescriptor descriptor)
    {
        var aggregate = new CredentialAggregate();

        var @event = new CredentialRegisteredEvent(id, descriptor);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // -- Activate ---------------------------------------------------------

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new CredentialActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // -- Revoke -----------------------------------------------------------

    public void Revoke()
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CredentialErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new CredentialRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // -- Apply ------------------------------------------------------------

    private void Apply(CredentialRegisteredEvent @event)
    {
        Id = @event.CredentialId;
        Descriptor = @event.Descriptor;
        Status = CredentialStatus.Registered;
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
            throw CredentialErrors.InvalidStateTransition(Status, "validate");
    }
}
