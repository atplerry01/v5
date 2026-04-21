using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Package;

public readonly record struct PackageId
{
    public Guid Value { get; }

    public PackageId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PackageId cannot be empty.");
        Value = value;
    }
}
