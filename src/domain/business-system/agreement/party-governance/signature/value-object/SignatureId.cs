namespace Whycespace.Domain.BusinessSystem.Agreement.PartyGovernance.Signature;

public readonly record struct SignatureId
{
    public Guid Value { get; }

    public SignatureId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SignatureId value must not be empty.", nameof(value));

        Value = value;
    }
}
