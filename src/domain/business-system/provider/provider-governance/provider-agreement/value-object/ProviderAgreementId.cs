namespace Whycespace.Domain.BusinessSystem.Provider.ProviderGovernance.ProviderAgreement;

public readonly record struct ProviderAgreementId
{
    public Guid Value { get; }

    public ProviderAgreementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderAgreementId value must not be empty.", nameof(value));

        Value = value;
    }
}
