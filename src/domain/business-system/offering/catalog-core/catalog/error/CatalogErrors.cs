namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public static class CatalogErrors
{
    public static CatalogDomainException MissingId()
        => new("CatalogId is required and must not be empty.");

    public static CatalogDomainException MissingStructure()
        => new("Catalog must have a structure definition.");

    public static CatalogDomainException InvalidStateTransition(CatalogStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CatalogDomainException MemberAlreadyPresent(CatalogMember member)
        => new($"Catalog already contains member '{member.Kind}:{member.MemberId}'.");

    public static CatalogDomainException MemberNotPresent(CatalogMember member)
        => new($"Catalog does not contain member '{member.Kind}:{member.MemberId}'.");

    public static CatalogDomainException ArchivedImmutable(CatalogId id)
        => new($"Catalog '{id.Value}' is archived and cannot be mutated.");
}

public sealed class CatalogDomainException : Exception
{
    public CatalogDomainException(string message) : base(message) { }
}
