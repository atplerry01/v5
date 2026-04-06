namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationMethod(string Value)
{
    public static readonly VerificationMethod OTP = new("OTP");
    public static readonly VerificationMethod Link = new("Link");
    public static readonly VerificationMethod DocumentUpload = new("DocumentUpload");
    public static readonly VerificationMethod BiometricScan = new("BiometricScan");
    public static readonly VerificationMethod ManualReview = new("ManualReview");

    public override string ToString() => Value;
}
