namespace Whycespace.Domain.BusinessSystem.Marketplace.Catalog;

public sealed class CatalogAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public CatalogId Id { get; private set; }
    public CatalogStatus Status { get; private set; }
    public CatalogStructure Structure { get; private set; }
    public int Version { get; private set; }

    private CatalogAggregate() { }

    public static CatalogAggregate Create(CatalogId id, CatalogStructure structure)
    {
        var aggregate = new CatalogAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CatalogCreatedEvent(id, structure);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Publish()
    {
        ValidateBeforeChange();

        var specification = new CanPublishSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CatalogErrors.InvalidStateTransition(Status, nameof(Publish));

        var @event = new CatalogPublishedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        ValidateBeforeChange();

        var specification = new CanArchiveSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CatalogErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new CatalogArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CatalogCreatedEvent @event)
    {
        Id = @event.CatalogId;
        Structure = @event.Structure;
        Status = CatalogStatus.Draft;
        Version++;
    }

    private void Apply(CatalogPublishedEvent @event)
    {
        Status = CatalogStatus.Published;
        Version++;
    }

    private void Apply(CatalogArchivedEvent @event)
    {
        Status = CatalogStatus.Archived;
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
            throw CatalogErrors.MissingId();

        if (Structure == default)
            throw CatalogErrors.MissingStructure();

        if (!Enum.IsDefined(Status))
            throw CatalogErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
