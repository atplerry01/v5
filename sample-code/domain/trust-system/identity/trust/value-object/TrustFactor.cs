namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustFactor(string Value)
{
    public static readonly TrustFactor EmailVerified = new("EmailVerified");
    public static readonly TrustFactor PhoneVerified = new("PhoneVerified");
    public static readonly TrustFactor KYCCompleted = new("KYCCompleted");
    public static readonly TrustFactor TwoFactorEnabled = new("TwoFactorEnabled");
    public static readonly TrustFactor TransactionHistory = new("TransactionHistory");
    public static readonly TrustFactor PeerVouch = new("PeerVouch");
    public static readonly TrustFactor Violation = new("Violation");

    public override string ToString() => Value;
}
