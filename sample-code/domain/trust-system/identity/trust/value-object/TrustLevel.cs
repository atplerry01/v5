namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed record TrustLevel(string Value)
{
    public static readonly TrustLevel Unverified = new("Unverified");
    public static readonly TrustLevel Low = new("Low");
    public static readonly TrustLevel Medium = new("Medium");
    public static readonly TrustLevel High = new("High");
    public static readonly TrustLevel Trusted = new("Trusted");

    public static TrustLevel FromScore(decimal score) => score switch
    {
        < 20m => Unverified,
        < 40m => Low,
        < 60m => Medium,
        < 80m => High,
        _ => Trusted
    };

    public override string ToString() => Value;
}
