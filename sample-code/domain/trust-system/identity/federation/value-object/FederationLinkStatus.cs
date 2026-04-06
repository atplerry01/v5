namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Status of a federation link between local and external identity.
/// </summary>
public sealed record FederationLinkStatus
{
    public string Value { get; }

    private FederationLinkStatus(string value) => Value = value;

    public static readonly FederationLinkStatus Active = new("Active");
    public static readonly FederationLinkStatus Suspended = new("Suspended");
    public static readonly FederationLinkStatus Revoked = new("Revoked");

    public bool IsActive => this == Active;

    public override string ToString() => Value;
}
