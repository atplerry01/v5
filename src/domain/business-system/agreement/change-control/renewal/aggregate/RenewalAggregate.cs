namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

public sealed class RenewalAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RenewalId Id { get; private set; }
    public RenewalSourceId SourceId { get; private set; }
    public RenewalStatus Status { get; private set; }
    public int Version { get; private set; }

    private RenewalAggregate() { }

    public static RenewalAggregate Create(RenewalId id, RenewalSourceId sourceId)
    {
        var aggregate = new RenewalAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RenewalCreatedEvent(id, sourceId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Renew()
    {
        ValidateBeforeChange();

        var specification = new CanRenewSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Renew));

        var @event = new RenewalRenewedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire()
    {
        ValidateBeforeChange();

        var specification = new CanExpireRenewalSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Expire));

        var @event = new RenewalExpiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RenewalCreatedEvent @event)
    {
        Id = @event.RenewalId;
        SourceId = @event.SourceId;
        Status = RenewalStatus.Pending;
        Version++;
    }

    private void Apply(RenewalRenewedEvent @event)
    {
        Status = RenewalStatus.Renewed;
        Version++;
    }

    private void Apply(RenewalExpiredEvent @event)
    {
        Status = RenewalStatus.Expired;
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
            throw RenewalErrors.MissingId();

        if (SourceId == default)
            throw RenewalErrors.MissingSourceId();

        if (!Enum.IsDefined(Status))
            throw RenewalErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
