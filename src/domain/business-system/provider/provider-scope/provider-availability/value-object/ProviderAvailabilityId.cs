using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public readonly record struct ProviderAvailabilityId
{
    public Guid Value { get; }

    public ProviderAvailabilityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProviderAvailabilityId cannot be empty.");
        Value = value;
    }
}
