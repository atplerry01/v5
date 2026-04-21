using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public enum BundleMemberKind
{
    Product,
    ServiceOffering
}

public readonly record struct BundleMember
{
    public BundleMemberKind Kind { get; }
    public BundleMemberId MemberId { get; }

    public BundleMember(BundleMemberKind kind, BundleMemberId memberId)
    {
        Guard.Against(!Enum.IsDefined(kind), "BundleMemberKind is invalid.");
        Guard.Against(memberId == default, "BundleMember MemberId must not be empty.");

        Kind = kind;
        MemberId = memberId;
    }
}
