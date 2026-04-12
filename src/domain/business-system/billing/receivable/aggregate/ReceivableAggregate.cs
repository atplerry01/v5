namespace Whycespace.Domain.BusinessSystem.Billing.Receivable;

public sealed class ReceivableAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReceivableId Id { get; private set; }
    public ReceivableStatus Status { get; private set; }
    public int Version { get; private set; }

    private ReceivableAggregate() { }

    public static ReceivableAggregate Create(ReceivableId id)
    {
        var aggregate = new ReceivableAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ReceivableCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Settle()
    {
        ValidateBeforeChange();

        var specification = new CanSettleSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReceivableErrors.InvalidStateTransition(Status, nameof(Settle));

        var @event = new ReceivableSettledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void WriteOff()
    {
        ValidateBeforeChange();

        var specification = new CanWriteOffSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReceivableErrors.InvalidStateTransition(Status, nameof(WriteOff));

        var @event = new ReceivableWrittenOffEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReceivableCreatedEvent @event)
    {
        Id = @event.ReceivableId;
        Status = ReceivableStatus.Outstanding;
        Version++;
    }

    private void Apply(ReceivableSettledEvent @event)
    {
        Status = ReceivableStatus.Settled;
        Version++;
    }

    private void Apply(ReceivableWrittenOffEvent @event)
    {
        Status = ReceivableStatus.WrittenOff;
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
            throw ReceivableErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ReceivableErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
