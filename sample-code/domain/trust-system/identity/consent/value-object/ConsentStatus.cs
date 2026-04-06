namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentStatus(string Value)
{
    public static readonly ConsentStatus Granted = new("Granted");
    public static readonly ConsentStatus Revoked = new("Revoked");
    public static readonly ConsentStatus Expired = new("Expired");
}
