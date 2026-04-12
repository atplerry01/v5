namespace Whycespace.Domain.BusinessSystem.Billing.Statement;

public sealed class StatementAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<StatementLine> _lines = new();

    public StatementId Id { get; private set; }
    public StatementStatus Status { get; private set; }
    public IReadOnlyList<StatementLine> Lines => _lines.AsReadOnly();
    public int Version { get; private set; }

    private StatementAggregate() { }

    public static StatementAggregate Create(StatementId id)
    {
        var aggregate = new StatementAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new StatementCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddLine(StatementLine line)
    {
        if (line is null)
            throw new ArgumentNullException(nameof(line));

        if (Status == StatementStatus.Closed)
            throw StatementErrors.InvalidStateTransition(Status, nameof(AddLine));

        _lines.Add(line);
    }

    public void IssueStatement()
    {
        ValidateBeforeChange();

        var specification = new CanIssueStatementSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StatementErrors.InvalidStateTransition(Status, nameof(IssueStatement));

        var nonEmptySpec = new IsNonEmptyStatementSpecification();
        if (!nonEmptySpec.IsSatisfiedBy(_lines.Count))
            throw StatementErrors.EmptyStatement();

        var @event = new StatementIssuedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void CloseStatement()
    {
        ValidateBeforeChange();

        var specification = new CanCloseStatementSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StatementErrors.InvalidStateTransition(Status, nameof(CloseStatement));

        var @event = new StatementClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(StatementCreatedEvent @event)
    {
        Id = @event.StatementId;
        Status = StatementStatus.Draft;
        Version++;
    }

    private void Apply(StatementIssuedEvent @event)
    {
        Status = StatementStatus.Issued;
        Version++;
    }

    private void Apply(StatementClosedEvent @event)
    {
        Status = StatementStatus.Closed;
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
            throw StatementErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw StatementErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
