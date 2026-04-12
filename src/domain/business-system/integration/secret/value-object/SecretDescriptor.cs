namespace Whycespace.Domain.BusinessSystem.Integration.Secret;

public readonly record struct SecretDescriptor
{
    public Guid OwnerReference { get; }
    public string SecretType { get; }

    public SecretDescriptor(Guid ownerReference, string secretType)
    {
        if (ownerReference == Guid.Empty)
            throw new ArgumentException("OwnerReference must not be empty.", nameof(ownerReference));
        if (string.IsNullOrWhiteSpace(secretType))
            throw new ArgumentException("SecretType must not be empty.", nameof(secretType));

        OwnerReference = ownerReference;
        SecretType = secretType;
    }
}
