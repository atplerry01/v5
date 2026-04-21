using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageMemberId
{
    public Guid Value { get; }

    public PackageMemberId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PackageMemberId cannot be empty.");
        Value = value;
    }
}
