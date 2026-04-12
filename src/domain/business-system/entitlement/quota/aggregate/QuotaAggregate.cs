namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public sealed class QuotaAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<QuotaUsage> _usages = new();

    public QuotaId Id { get; private set; }
    public QuotaSubjectId SubjectId { get; private set; }
    public QuotaStatus Status { get; private set; }
    public int TotalCapacity { get; private set; }
    public int TotalConsumed { get; private set; }
    public IReadOnlyList<QuotaUsage> Usages => _usages.AsReadOnly();
    public int Remaining => TotalCapacity - TotalConsumed;
    public int Version { get; private set; }

    private QuotaAggregate() { }

    public static QuotaAggregate Create(QuotaId id, QuotaSubjectId subjectId, int totalCapacity)
    {
        if (totalCapacity <= 0)
            throw new ArgumentException("Total capacity must be greater than zero.", nameof(totalCapacity));

        var aggregate = new QuotaAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new QuotaCreatedEvent(id, subjectId, totalCapacity);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Consume(QuotaUsage usage)
    {
        ValidateBeforeChange();

        if (usage is null)
            throw new ArgumentNullException(nameof(usage));

        var specification = new CanConsumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuotaErrors.InvalidStateTransition(Status, nameof(Consume));

        if (usage.UnitsConsumed > Remaining)
            throw QuotaErrors.CapacityExceeded(usage.UnitsConsumed, Remaining);

        var @event = new QuotaConsumedEvent(Id, usage.UsageId, usage.UnitsConsumed);
        Apply(@event, usage);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Exhaust()
    {
        ValidateBeforeChange();

        var specification = new CanExhaustSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw QuotaErrors.InvalidStateTransition(Status, nameof(Exhaust));

        if (Remaining > 0)
            throw QuotaErrors.CapacityRemaining(Remaining);

        var @event = new QuotaExhaustedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(QuotaCreatedEvent @event)
    {
        Id = @event.QuotaId;
        SubjectId = @event.SubjectId;
        TotalCapacity = @event.TotalCapacity;
        TotalConsumed = 0;
        Status = QuotaStatus.Available;
        Version++;
    }

    private void Apply(QuotaConsumedEvent @event, QuotaUsage usage)
    {
        _usages.Add(usage);
        TotalConsumed += @event.UnitsConsumed;
        Status = QuotaStatus.Consumed;
        Version++;
    }

    private void Apply(QuotaExhaustedEvent @event)
    {
        Status = QuotaStatus.Exhausted;
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
            throw QuotaErrors.MissingId();

        if (SubjectId == default)
            throw QuotaErrors.MissingSubjectId();

        if (TotalCapacity <= 0)
            throw QuotaErrors.InvalidCapacity();

        if (TotalConsumed > TotalCapacity)
            throw QuotaErrors.ConsumptionExceedsCapacity(TotalConsumed, TotalCapacity);

        if (!Enum.IsDefined(Status))
            throw QuotaErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
