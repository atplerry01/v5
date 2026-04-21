using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public readonly record struct ProviderAgreementId
{
    public Guid Value { get; }

    public ProviderAgreementId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProviderAgreementId cannot be empty.");
        Value = value;
    }
}
