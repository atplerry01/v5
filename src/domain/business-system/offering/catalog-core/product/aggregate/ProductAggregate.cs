namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed class ProductAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProductId Id { get; private set; }
    public ProductName Name { get; private set; }
    public ProductType Type { get; private set; }
    public ProductStatus Status { get; private set; }
    public CatalogRef? Catalog { get; private set; }
    public int Version { get; private set; }

    private ProductAggregate() { }

    public static ProductAggregate Create(
        ProductId id,
        ProductName name,
        ProductType type,
        CatalogRef? catalog = null)
    {
        var aggregate = new ProductAggregate();

        var @event = new ProductCreatedEvent(id, name, type, catalog);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Update(ProductName name, ProductType type)
    {
        EnsureMutable(nameof(Update));

        var @event = new ProductUpdatedEvent(Id, name, type);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProductErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProductActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
            throw ProductErrors.InvalidStateTransition(Status, nameof(Archive));

        var @event = new ProductArchivedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProductCreatedEvent @event)
    {
        Id = @event.ProductId;
        Name = @event.Name;
        Type = @event.Type;
        Catalog = @event.Catalog;
        Status = ProductStatus.Draft;
        Version++;
    }

    private void Apply(ProductUpdatedEvent @event)
    {
        Name = @event.Name;
        Type = @event.Type;
        Version++;
    }

    private void Apply(ProductActivatedEvent @event)
    {
        Status = ProductStatus.Active;
        Version++;
    }

    private void Apply(ProductArchivedEvent @event)
    {
        Status = ProductStatus.Archived;
        Version++;
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProductErrors.ArchivedImmutable(Id);
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProductErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ProductErrors.InvalidStateTransition(Status, "validate");
    }
}
