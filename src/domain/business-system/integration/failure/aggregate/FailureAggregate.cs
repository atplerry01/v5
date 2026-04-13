namespace Whycespace.Domain.BusinessSystem.Integration.Failure;

public sealed class FailureAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public FailureId Id { get; private set; }
    public FailureTypeId TypeId { get; private set; }
    public FailureStatus Status { get; private set; }
    public string Classification { get; private set; } = null!;
    public int Version { get; private set; }

    private FailureAggregate() { }

    public static FailureAggregate Create(FailureId id, FailureTypeId typeId)
    {
        var aggregate = new FailureAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new FailureDetectedEvent(id, typeId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Classify(string classification)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(classification))
            throw new ArgumentException("Classification must not be empty.", nameof(classification));

        var specification = new CanClassifySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FailureErrors.InvalidStateTransition(Status, nameof(Classify));

        var @event = new FailureClassifiedEvent(Id, classification);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resolve()
    {
        ValidateBeforeChange();

        var specification = new CanResolveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw FailureErrors.InvalidStateTransition(Status, nameof(Resolve));

        var @event = new FailureResolvedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(FailureDetectedEvent @event)
    {
        Id = @event.FailureId;
        TypeId = @event.TypeId;
        Status = FailureStatus.Detected;
        Version++;
    }

    private void Apply(FailureClassifiedEvent @event)
    {
        Status = FailureStatus.Classified;
        Classification = @event.Classification;
        Version++;
    }

    private void Apply(FailureResolvedEvent @event)
    {
        Status = FailureStatus.Resolved;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw FailureErrors.MissingId();

        if (TypeId == default)
            throw FailureErrors.MissingTypeId();

        if (!Enum.IsDefined(Status))
            throw FailureErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
