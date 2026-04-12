namespace Whycespace.Domain.BusinessSystem.Execution.Sourcing;

public sealed class SourcingAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SourcingId Id { get; private set; }
    public SourcingRequestId RequestId { get; private set; }
    public SourcingStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public int Version { get; private set; }

    private SourcingAggregate() { }

    public static SourcingAggregate Create(SourcingId id, SourcingRequestId requestId)
    {
        var aggregate = new SourcingAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SourcingCreatedEvent(id, requestId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Source()
    {
        ValidateBeforeChange();

        var specification = new CanSourceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SourcingErrors.InvalidStateTransition(Status, nameof(Source));

        var @event = new SourcingSourcedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Fail(string reason)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason must not be empty.", nameof(reason));

        var specification = new CanFailSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SourcingErrors.InvalidStateTransition(Status, nameof(Fail));

        var @event = new SourcingFailedEvent(Id, reason);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SourcingCreatedEvent @event)
    {
        Id = @event.SourcingId;
        RequestId = @event.RequestId;
        Status = SourcingStatus.Requested;
        Version++;
    }

    private void Apply(SourcingSourcedEvent @event)
    {
        Status = SourcingStatus.Sourced;
        Version++;
    }

    private void Apply(SourcingFailedEvent @event)
    {
        Status = SourcingStatus.Failed;
        FailureReason = @event.Reason;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SourcingErrors.MissingId();
        if (RequestId == default)
            throw SourcingErrors.MissingRequestId();
        if (!Enum.IsDefined(Status))
            throw SourcingErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
