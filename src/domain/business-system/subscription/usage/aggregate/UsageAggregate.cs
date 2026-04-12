namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public sealed class UsageAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public UsageId Id { get; private set; }
    public UsageRecord UsageRecord { get; private set; }
    public UsageStatus Status { get; private set; }
    public int Version { get; private set; }

    private UsageAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static UsageAggregate RecordUsage(UsageId id, UsageRecord record)
    {
        var aggregate = new UsageAggregate();

        var @event = new UsageRecordedEvent(id, record);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    // ── Aggregate ────────────────────────────────────────────────

    public void AggregateUsage()
    {
        var specification = new CanAggregateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageErrors.InvalidStateTransition(Status, nameof(AggregateUsage));

        var @event = new UsageAggregatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Finalize ─────────────────────────────────────────────────

    public void FinalizeUsage()
    {
        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageErrors.InvalidStateTransition(Status, nameof(FinalizeUsage));

        var @event = new UsageFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    // ── Apply ────────────────────────────────────────────────────

    private void Apply(UsageRecordedEvent @event)
    {
        Id = @event.UsageId;
        UsageRecord = @event.UsageRecord;
        Status = UsageStatus.Recorded;
        Version++;
    }

    private void Apply(UsageAggregatedEvent @event)
    {
        Status = UsageStatus.Aggregated;
        Version++;
    }

    private void Apply(UsageFinalizedEvent @event)
    {
        Status = UsageStatus.Finalized;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw UsageErrors.MissingId();

        if (UsageRecord == default)
            throw UsageErrors.MissingRecord();

        if (!Enum.IsDefined(Status))
            throw UsageErrors.InvalidStateTransition(Status, "validate");
    }
}
