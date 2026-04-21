using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct OfferingPackageRef
{
    public Guid Value { get; }

    public OfferingPackageRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OfferingPackageRef cannot be empty.");
        Value = value;
    }
}
