using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed class ProductAggregate : AggregateRoot
{
    public ProductId Id { get; private set; }
    public ProductName Name { get; private set; }
    public ProductType Type { get; private set; }
    public ProductStatus Status { get; private set; }
    public CatalogRef? Catalog { get; private set; }

    public static ProductAggregate Create(
        ProductId id,
        ProductName name,
        ProductType type,
        CatalogRef? catalog = null)
    {
        var aggregate = new ProductAggregate();
        if (aggregate.Version >= 0)
            throw ProductErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ProductCreatedEvent(id, name, type, catalog));
        return aggregate;
    }

    public void Update(ProductName name, ProductType type)
    {
        EnsureMutable(nameof(Update));

        RaiseDomainEvent(new ProductUpdatedEvent(Id, name, type));
    }

    public void Activate()
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProductErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ProductActivatedEvent(Id));
    }

    public void Archive()
    {
        if (Status == ProductStatus.Archived)
            throw ProductErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ProductArchivedEvent(Id));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ProductCreatedEvent e:
                Id = e.ProductId;
                Name = e.Name;
                Type = e.Type;
                Catalog = e.Catalog;
                Status = ProductStatus.Draft;
                break;
            case ProductUpdatedEvent e:
                Name = e.Name;
                Type = e.Type;
                break;
            case ProductActivatedEvent:
                Status = ProductStatus.Active;
                break;
            case ProductArchivedEvent:
                Status = ProductStatus.Archived;
                break;
        }
    }

    private void EnsureMutable(string attemptedAction)
    {
        var specification = new CanMutateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProductErrors.ArchivedImmutable(Id);
    }

    protected override void EnsureInvariants()
    {
        if (Id == default)
            throw ProductErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ProductErrors.InvalidStateTransition(Status, "validate");
    }
}
