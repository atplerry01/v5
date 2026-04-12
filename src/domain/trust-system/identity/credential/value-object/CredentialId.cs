namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public readonly record struct CredentialId
{
    public Guid Value { get; }

    public CredentialId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CredentialId value must not be empty.", nameof(value));
        Value = value;
    }
}
