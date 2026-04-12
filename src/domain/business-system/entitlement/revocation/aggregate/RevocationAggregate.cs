namespace Whycespace.Domain.BusinessSystem.Entitlement.Revocation;

public sealed class RevocationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RevocationId Id { get; private set; }
    public RevocationTargetId TargetId { get; private set; }
    public RevocationStatus Status { get; private set; }
    public string RevocationReason { get; private set; }
    public int Version { get; private set; }

    private RevocationAggregate() { }

    public static RevocationAggregate Create(RevocationId id, RevocationTargetId targetId)
    {
        var aggregate = new RevocationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RevocationCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Revoke(string reason)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Revocation reason must not be empty.", nameof(reason));

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RevocationErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new RevocationRevokedEvent(Id, reason);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Finalize()
    {
        ValidateBeforeChange();

        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RevocationErrors.InvalidStateTransition(Status, nameof(Finalize));

        var @event = new RevocationFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RevocationCreatedEvent @event)
    {
        Id = @event.RevocationId;
        TargetId = @event.TargetId;
        Status = RevocationStatus.Active;
        Version++;
    }

    private void Apply(RevocationRevokedEvent @event)
    {
        Status = RevocationStatus.Revoked;
        RevocationReason = @event.Reason;
        Version++;
    }

    private void Apply(RevocationFinalizedEvent @event)
    {
        Status = RevocationStatus.Finalized;
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
            throw RevocationErrors.MissingId();

        if (TargetId == default)
            throw RevocationErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw RevocationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
