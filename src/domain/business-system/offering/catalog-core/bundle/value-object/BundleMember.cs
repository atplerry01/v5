namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public enum BundleMemberKind
{
    Product,
    ServiceOffering
}

public readonly record struct BundleMember
{
    public BundleMemberKind Kind { get; }
    public Guid MemberId { get; }

    public BundleMember(BundleMemberKind kind, Guid memberId)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("BundleMemberKind is invalid.", nameof(kind));

        if (memberId == Guid.Empty)
            throw new ArgumentException("BundleMember MemberId must not be empty.", nameof(memberId));

        Kind = kind;
        MemberId = memberId;
    }
}
