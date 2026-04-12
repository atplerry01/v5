namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public readonly record struct CredentialId
{
    public Guid Value { get; }

    public CredentialId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CredentialId cannot be empty.", nameof(value));

        Value = value;
    }
}
