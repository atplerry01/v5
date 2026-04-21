using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public readonly record struct ServiceOfferingId
{
    public Guid Value { get; }

    public ServiceOfferingId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ServiceOfferingId cannot be empty.");
        Value = value;
    }
}
