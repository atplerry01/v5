using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public readonly record struct ContractRef
{
    public Guid Value { get; }

    public ContractRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ContractRef cannot be empty.");
        Value = value;
    }
}
