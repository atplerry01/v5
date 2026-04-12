namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public sealed class BenchmarkAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public BenchmarkId Id { get; private set; }
    public BenchmarkName Name { get; private set; }
    public BenchmarkStatus Status { get; private set; }
    public int Version { get; private set; }

    private BenchmarkAggregate() { }

    public static BenchmarkAggregate Create(
        BenchmarkId id,
        BenchmarkName name)
    {
        var aggregate = new BenchmarkAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new BenchmarkCreatedEvent(id, name);
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
            throw BenchmarkErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new BenchmarkActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Retire()
    {
        ValidateBeforeChange();

        var specification = new CanRetireSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BenchmarkErrors.InvalidStateTransition(Status, nameof(Retire));

        var @event = new BenchmarkRetiredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BenchmarkCreatedEvent @event)
    {
        Id = @event.BenchmarkId;
        Name = @event.BenchmarkName;
        Status = BenchmarkStatus.Draft;
        Version++;
    }

    private void Apply(BenchmarkActivatedEvent @event)
    {
        Status = BenchmarkStatus.Active;
        Version++;
    }

    private void Apply(BenchmarkRetiredEvent @event)
    {
        Status = BenchmarkStatus.Retired;
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
            throw BenchmarkErrors.MissingId();

        if (Name == default)
            throw BenchmarkErrors.NameRequired();

        if (!Enum.IsDefined(Status))
            throw BenchmarkErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
