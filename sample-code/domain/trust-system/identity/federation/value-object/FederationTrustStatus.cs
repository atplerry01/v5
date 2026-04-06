namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Status of the trust relationship between WhyceID and an issuer.
/// </summary>
public sealed record FederationTrustStatus
{
    public string Value { get; }

    private FederationTrustStatus(string value) => Value = value;

    public static readonly FederationTrustStatus Active = new("Active");
    public static readonly FederationTrustStatus Degraded = new("Degraded");
    public static readonly FederationTrustStatus Suspended = new("Suspended");

    public override string ToString() => Value;
}
