namespace Whycespace.Domain.BusinessSystem.Portfolio.Exposure;

public sealed class ExposureAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ExposureId Id { get; private set; }
    public ExposureContext ExposureContext { get; private set; }
    public ExposureLimit Limit { get; private set; }
    public ExposureStatus Status { get; private set; }
    public int Version { get; private set; }

    private ExposureAggregate() { }

    public static ExposureAggregate Create(
        ExposureId id,
        ExposureContext exposureContext,
        ExposureLimit limit)
    {
        var aggregate = new ExposureAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ExposureCreatedEvent(id, exposureContext, limit);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateExposureSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ExposureErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ExposureActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Breach()
    {
        ValidateBeforeChange();

        var specification = new CanBreachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ExposureErrors.InvalidStateTransition(Status, nameof(Breach));

        var @event = new ExposureBreachedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Clear()
    {
        ValidateBeforeChange();

        var specification = new CanClearSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ExposureErrors.InvalidStateTransition(Status, nameof(Clear));

        var @event = new ExposureClearedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ExposureCreatedEvent @event)
    {
        Id = @event.ExposureId;
        ExposureContext = @event.ExposureContext;
        Limit = @event.Limit;
        Status = ExposureStatus.Defined;
        Version++;
    }

    private void Apply(ExposureActivatedEvent @event)
    {
        Status = ExposureStatus.Active;
        Version++;
    }

    private void Apply(ExposureBreachedEvent @event)
    {
        Status = ExposureStatus.Breached;
        Version++;
    }

    private void Apply(ExposureClearedEvent @event)
    {
        Status = ExposureStatus.Cleared;
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
            throw ExposureErrors.MissingId();

        if (ExposureContext == default)
            throw ExposureErrors.ContextRequired();

        if (Limit == default)
            throw ExposureErrors.LimitRequired();

        if (!Enum.IsDefined(Status))
            throw ExposureErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
