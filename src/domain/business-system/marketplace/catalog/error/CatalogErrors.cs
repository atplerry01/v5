namespace Whycespace.Domain.BusinessSystem.Marketplace.Catalog;

public static class CatalogErrors
{
    public static CatalogDomainException MissingId()
        => new("CatalogId is required and must not be empty.");

    public static CatalogDomainException MissingStructure()
        => new("Catalog must have a structure definition.");

    public static CatalogDomainException InvalidStateTransition(CatalogStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}

public sealed class CatalogDomainException : Exception
{
    public CatalogDomainException(string message) : base(message) { }
}
