namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public readonly record struct ContractRef
{
    public Guid Value { get; }

    public ContractRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContractRef value must not be empty.", nameof(value));

        Value = value;
    }
}
