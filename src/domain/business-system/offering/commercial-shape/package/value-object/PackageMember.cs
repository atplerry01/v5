using Whycespace.Domain.SharedKernel.Primitives.Kernel;

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
    public PackageMemberId MemberId { get; }

    public PackageMember(PackageMemberKind kind, PackageMemberId memberId)
    {
        Guard.Against(!Enum.IsDefined(kind), "PackageMemberKind is invalid.");
        Guard.Against(memberId == default, "PackageMember MemberId must not be empty.");

        Kind = kind;
        MemberId = memberId;
    }
}
