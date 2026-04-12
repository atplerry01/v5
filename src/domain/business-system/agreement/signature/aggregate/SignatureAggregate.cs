namespace Whycespace.Domain.BusinessSystem.Agreement.Signature;

public sealed class SignatureAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SignatureId Id { get; private set; }
    public SignatureStatus Status { get; private set; }
    public int Version { get; private set; }

    private SignatureAggregate() { }

    public static SignatureAggregate Create(SignatureId id)
    {
        var aggregate = new SignatureAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SignatureCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Sign()
    {
        ValidateBeforeChange();

        var specification = new CanSignSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureErrors.InvalidStateTransition(Status, nameof(Sign));

        var @event = new SignatureSignedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke()
    {
        ValidateBeforeChange();

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SignatureErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new SignatureRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SignatureCreatedEvent @event)
    {
        Id = @event.SignatureId;
        Status = SignatureStatus.Pending;
        Version++;
    }

    private void Apply(SignatureSignedEvent @event)
    {
        Status = SignatureStatus.Signed;
        Version++;
    }

    private void Apply(SignatureRevokedEvent @event)
    {
        Status = SignatureStatus.Revoked;
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
            throw SignatureErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SignatureErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
