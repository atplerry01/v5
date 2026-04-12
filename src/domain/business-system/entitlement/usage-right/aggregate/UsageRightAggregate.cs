namespace Whycespace.Domain.BusinessSystem.Entitlement.UsageRight;

public sealed class UsageRightAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<UsageRecord> _usageRecords = new();

    public UsageRightId Id { get; private set; }
    public UsageRightSubjectId SubjectId { get; private set; }
    public UsageRightReferenceId ReferenceId { get; private set; }
    public UsageRightStatus Status { get; private set; }
    public int TotalUnits { get; private set; }
    public int TotalUsed { get; private set; }
    public IReadOnlyList<UsageRecord> UsageRecords => _usageRecords.AsReadOnly();
    public int Remaining => TotalUnits - TotalUsed;
    public int Version { get; private set; }

    private UsageRightAggregate() { }

    public static UsageRightAggregate Create(UsageRightId id, UsageRightSubjectId subjectId, UsageRightReferenceId referenceId, int totalUnits)
    {
        if (totalUnits <= 0)
            throw new ArgumentException("Total units must be greater than zero.", nameof(totalUnits));

        var aggregate = new UsageRightAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new UsageRightCreatedEvent(id, subjectId, referenceId, totalUnits);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Use(UsageRecord record)
    {
        ValidateBeforeChange();

        if (record is null)
            throw new ArgumentNullException(nameof(record));

        var specification = new CanUseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, nameof(Use));

        if (record.UnitsUsed > Remaining)
            throw UsageRightErrors.UsageExceedsAvailable(record.UnitsUsed, Remaining);

        var @event = new UsageRightUsedEvent(Id, record.RecordId, record.UnitsUsed);
        Apply(@event, record);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Consume()
    {
        ValidateBeforeChange();

        var specification = new CanConsumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, nameof(Consume));

        if (Remaining > 0)
            throw UsageRightErrors.UsageRemaining(Remaining);

        var @event = new UsageRightConsumedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(UsageRightCreatedEvent @event)
    {
        Id = @event.UsageRightId;
        SubjectId = @event.SubjectId;
        ReferenceId = @event.ReferenceId;
        TotalUnits = @event.TotalUnits;
        TotalUsed = 0;
        Status = UsageRightStatus.Available;
        Version++;
    }

    private void Apply(UsageRightUsedEvent @event, UsageRecord record)
    {
        _usageRecords.Add(record);
        TotalUsed += @event.UnitsUsed;
        Status = UsageRightStatus.InUse;
        Version++;
    }

    private void Apply(UsageRightConsumedEvent @event)
    {
        Status = UsageRightStatus.Consumed;
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
            throw UsageRightErrors.MissingId();

        if (SubjectId == default)
            throw UsageRightErrors.MissingSubjectId();

        if (ReferenceId == default)
            throw UsageRightErrors.MissingReferenceId();

        if (TotalUnits <= 0)
            throw UsageRightErrors.InvalidTotalUnits();

        if (!Enum.IsDefined(Status))
            throw UsageRightErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
