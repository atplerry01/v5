namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Identifier for an identity in an external system.
/// Opaque string — format depends on the issuer.
/// </summary>
public sealed record ExternalIdentityId
{
    public string Value { get; }

    public ExternalIdentityId(string value)
    {
        Guard.AgainstEmpty(value);
        Value = value;
    }

    public override string ToString() => Value;
}
