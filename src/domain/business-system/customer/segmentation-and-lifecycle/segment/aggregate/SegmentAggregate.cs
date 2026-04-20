namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public sealed class SegmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SegmentId Id { get; private set; }
    public SegmentCode Code { get; private set; }
    public SegmentName Name { get; private set; }
    public SegmentType Type { get; private set; }
    public SegmentCriteria Criteria { get; private set; }
    public SegmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private SegmentAggregate() { }

    public static SegmentAggregate Create(
        SegmentId id,
        SegmentCode code,
        SegmentName name,
        SegmentType type,
        SegmentCriteria criteria)
    {
        var aggregate = new SegmentAggregate();

        var @event = new SegmentCreatedEvent(id, code, name, type, criteria);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(SegmentName name, SegmentCriteria criteria)
    {
        EnsureMutable();

        var @event = new SegmentUpdatedEvent(Id, name, criteria);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SegmentErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SegmentActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == SegmentStatus.Archived)
            throw SegmentErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new SegmentArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SegmentCreatedEvent @event)
    {
        Id = @event.SegmentId;
        Code = @event.Code;
        Name = @event.Name;
        Type = @event.Type;
        Criteria = @event.Criteria;
        Status = SegmentStatus.Draft;
        Version++;
    }

    private void Apply(SegmentUpdatedEvent @event)
    {
        Name = @event.Name;
        Criteria = @event.Criteria;
        Version++;
    }

    private void Apply(SegmentActivatedEvent @event)
    {
        Status = SegmentStatus.Active;
        Version++;
    }

    private void Apply(SegmentArchivedEvent @event)
    {
        Status = SegmentStatus.Archived;
        Version++;
    }

    private void EnsureMutable()
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SegmentErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SegmentErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw SegmentErrors.InvalidStateTransition(Status, "validate");
    }
}
