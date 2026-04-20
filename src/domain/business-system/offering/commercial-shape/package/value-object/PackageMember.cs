namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public enum PackageMemberKind
{
    Product,
    ServiceOffering,
    Bundle,
    Plan
}

public readonly record struct PackageMember
{
    public PackageMemberKind Kind { get; }
    public Guid MemberId { get; }

    public PackageMember(PackageMemberKind kind, Guid memberId)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentException("PackageMemberKind is invalid.", nameof(kind));

        if (memberId == Guid.Empty)
            throw new ArgumentException("PackageMember MemberId must not be empty.", nameof(memberId));

        Kind = kind;
        MemberId = memberId;
    }
}
