namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentType(string Value)
{
    public static readonly ConsentType KYC = new("KYC");
    public static readonly ConsentType DataProcessing = new("DataProcessing");
    public static readonly ConsentType Marketing = new("Marketing");
    public static readonly ConsentType ThirdPartySharing = new("ThirdPartySharing");
    public static readonly ConsentType Analytics = new("Analytics");
}
