namespace Whycespace.Domain.TrustSystem.Identity.Credential;

/// <summary>
/// Wraps a stored credential hash. Enforces that the value is non-empty and
/// meets minimum length for a cryptographic hash (≥ 20 chars), preventing
/// accidental plaintext storage at the domain boundary.
/// </summary>
public readonly record struct CredentialHashValue
{
    public string Value { get; }

    public CredentialHashValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Credential hash must not be empty.", nameof(value));
        if (value.Length < 20)
            throw new ArgumentException("Credential hash must be at least 20 characters — plaintext credentials are not permitted.", nameof(value));

        Value = value;
    }

    public static implicit operator string(CredentialHashValue hash) => hash.Value;
}
