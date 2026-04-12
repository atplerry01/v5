namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public readonly record struct SecretId
{
    public Guid Value { get; }

    public SecretId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SecretId value must not be empty.", nameof(value));
        Value = value;
    }
}
