namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationType(string Value)
{
    public static readonly VerificationType Email = new("Email");
    public static readonly VerificationType Phone = new("Phone");
    public static readonly VerificationType KYC = new("KYC");
    public static readonly VerificationType Document = new("Document");
    public static readonly VerificationType Biometric = new("Biometric");

    public override string ToString() => Value;
}
