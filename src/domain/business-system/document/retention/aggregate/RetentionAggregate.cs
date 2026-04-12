namespace Whycespace.Domain.BusinessSystem.Document.Retention;

public sealed class RetentionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RetentionId Id { get; private set; }
    public RetentionStatus Status { get; private set; }
    public int Version { get; private set; }

    private RetentionAggregate() { }

    public static RetentionAggregate Create(RetentionId id)
    {
        var aggregate = new RetentionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RetentionCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Retain()
    {
        ValidateBeforeChange();

        var specification = new CanRetainSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RetentionErrors.InvalidStateTransition(Status, nameof(Retain));

        var @event = new RetentionRetainedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Expire()
    {
        ValidateBeforeChange();

        var specification = new CanExpireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RetentionErrors.InvalidStateTransition(Status, nameof(Expire));

        var isRetained = new IsRetainedSpecification();
        if (!isRetained.IsSatisfiedBy(Status))
            throw RetentionErrors.RetentionConditionNotMet();

        var @event = new RetentionExpiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RetentionCreatedEvent @event)
    {
        Id = @event.RetentionId;
        Status = RetentionStatus.Active;
        Version++;
    }

    private void Apply(RetentionRetainedEvent @event)
    {
        Status = RetentionStatus.Retained;
        Version++;
    }

    private void Apply(RetentionExpiredEvent @event)
    {
        Status = RetentionStatus.Expired;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RetentionErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw RetentionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-change validation gate
        // POLICY HOOK (to be enforced by runtime)
    }
}
