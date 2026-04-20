using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingStructure.PriceBook;

public sealed class PriceBookAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PriceBookId Id { get; private set; }
    public PriceBookName Name { get; private set; }
    public PriceBookStatus Status { get; private set; }
    public PriceBookScopeRef? Scope { get; private set; }
    public TimeWindow? Effective { get; private set; }
    public int Version { get; private set; }

    private PriceBookAggregate() { }

    public static PriceBookAggregate Create(
        PriceBookId id,
        PriceBookName name,
        PriceBookScopeRef? scope = null,
        TimeWindow? effective = null)
    {
        var aggregate = new PriceBookAggregate();

        var @event = new PriceBookCreatedEvent(id, name, scope, effective);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate(TimeWindow effective)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PriceBookActivatedEvent(Id, effective);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deprecate()
    {
        var specification = new CanDeprecateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Deprecate));

        var @event = new PriceBookDeprecatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new PriceBookArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PriceBookCreatedEvent @event)
    {
        Id = @event.PriceBookId;
        Name = @event.Name;
        Scope = @event.Scope;
        Effective = @event.Effective;
        Status = PriceBookStatus.Draft;
        Version++;
    }

    private void Apply(PriceBookActivatedEvent @event)
    {
        Effective = @event.Effective;
        Status = PriceBookStatus.Active;
        Version++;
    }

    private void Apply(PriceBookDeprecatedEvent @event)
    {
        Status = PriceBookStatus.Deprecated;
        Version++;
    }

    private void Apply(PriceBookArchivedEvent @event)
    {
        Status = PriceBookStatus.Archived;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw PriceBookErrors.MissingId();

        if (Status == PriceBookStatus.Active && Effective is null)
            throw PriceBookErrors.EffectiveWindowRequiredForActivation();

        if (!Enum.IsDefined(Status))
            throw PriceBookErrors.InvalidStateTransition(Status, "validate");
    }
}
