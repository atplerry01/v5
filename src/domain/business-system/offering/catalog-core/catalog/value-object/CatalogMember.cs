namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public enum CatalogMemberKind
{
    Product,
    ServiceOffering,
    Bundle
}

public readonly record struct CatalogMember
{
    public CatalogMemberKind Kind { get; }
    public Guid MemberId { get; }

    public CatalogMember(CatalogMemberKind kind, Guid memberId)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("CatalogMemberKind is invalid.", nameof(kind));

        if (memberId == Guid.Empty)
            throw new ArgumentException("CatalogMember MemberId must not be empty.", nameof(memberId));

        Kind = kind;
        MemberId = memberId;
    }
}
