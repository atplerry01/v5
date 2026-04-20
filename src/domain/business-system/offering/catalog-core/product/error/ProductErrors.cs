namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public static class ProductErrors
{
    public static ProductDomainException MissingId()
        => new("ProductId is required and must not be empty.");

    public static ProductDomainException InvalidStateTransition(ProductStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static ProductDomainException ArchivedImmutable(ProductId id)
        => new($"Product '{id.Value}' is archived and cannot be mutated.");
}

public sealed class ProductDomainException : Exception
{
    public ProductDomainException(string message) : base(message) { }
}
