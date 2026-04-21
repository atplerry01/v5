using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public readonly record struct ProviderTierId
{
    public Guid Value { get; }

    public ProviderTierId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProviderTierId cannot be empty.");
        Value = value;
    }
}
