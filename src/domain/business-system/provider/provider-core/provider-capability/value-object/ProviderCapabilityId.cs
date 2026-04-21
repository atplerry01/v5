using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public readonly record struct ProviderCapabilityId
{
    public Guid Value { get; }

    public ProviderCapabilityId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProviderCapabilityId cannot be empty.");
        Value = value;
    }
}
