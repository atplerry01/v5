using Whycespace.Domain.SharedKernel.Primitives.Kernel;

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
    public CatalogMemberId MemberId { get; }

    public CatalogMember(CatalogMemberKind kind, CatalogMemberId memberId)
    {
        Guard.Against(!Enum.IsDefined(kind), "CatalogMemberKind is invalid.");
        Guard.Against(memberId == default, "CatalogMember MemberId must not be empty.");

        Kind = kind;
        MemberId = memberId;
    }
}
