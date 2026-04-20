namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Clause;

public sealed class ClauseAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ClauseId Id { get; private set; }
    public ClauseType Type { get; private set; }
    public ClauseStatus Status { get; private set; }
    public int Version { get; private set; }

    private ClauseAggregate() { }

    public static ClauseAggregate Create(ClauseId id, ClauseType clauseType)
    {
        var aggregate = new ClauseAggregate();
        aggregate.ValidateBeforeChange();

        var validationSpec = new IsValidClauseSpecification();
        if (!validationSpec.IsSatisfiedBy(id, clauseType))
            throw ClauseErrors.InvalidClauseType();

        var @event = new ClauseCreatedEvent(id, clauseType);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateClauseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClauseErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ClauseActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Supersede()
    {
        ValidateBeforeChange();

        var specification = new CanSupersedeClauseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ClauseErrors.InvalidStateTransition(Status, nameof(Supersede));

        var @event = new ClauseSupersededEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ClauseCreatedEvent @event)
    {
        Id = @event.ClauseId;
        Type = @event.ClauseType;
        Status = ClauseStatus.Draft;
        Version++;
    }

    private void Apply(ClauseActivatedEvent @event)
    {
        Status = ClauseStatus.Active;
        Version++;
    }

    private void Apply(ClauseSupersededEvent @event)
    {
        Status = ClauseStatus.Superseded;
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
            throw ClauseErrors.MissingId();

        if (!Enum.IsDefined(Type))
            throw ClauseErrors.InvalidClauseType();

        if (!Enum.IsDefined(Status))
            throw ClauseErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
