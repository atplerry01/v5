namespace Whycespace.Domain.BusinessSystem.Portfolio.Portfolio;

public sealed class PortfolioAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PortfolioId Id { get; private set; }
    public PortfolioName Name { get; private set; }
    public PortfolioStatus Status { get; private set; }
    public int Version { get; private set; }

    private PortfolioAggregate() { }

    public static PortfolioAggregate Create(
        PortfolioId id,
        PortfolioName name)
    {
        var aggregate = new PortfolioAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new PortfolioCreatedEvent(id, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PortfolioErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PortfolioActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PortfolioErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new PortfolioClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PortfolioErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new PortfolioTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PortfolioCreatedEvent @event)
    {
        Id = @event.PortfolioId;
        Name = @event.PortfolioName;
        Status = PortfolioStatus.Draft;
        Version++;
    }

    private void Apply(PortfolioActivatedEvent @event)
    {
        Status = PortfolioStatus.Active;
        Version++;
    }

    private void Apply(PortfolioClosedEvent @event)
    {
        Status = PortfolioStatus.Closed;
        Version++;
    }

    private void Apply(PortfolioTerminatedEvent @event)
    {
        Status = PortfolioStatus.Terminated;
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
            throw PortfolioErrors.MissingId();

        if (Name == default)
            throw PortfolioErrors.NameRequired();

        if (!Enum.IsDefined(Status))
            throw PortfolioErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
