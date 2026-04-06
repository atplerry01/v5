namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Level of identity verification performed by the issuer.
/// Higher = more rigorous verification process.
/// </summary>
public sealed record VerificationLevel
{
    public int Value { get; }

    public VerificationLevel(int value)
    {
        if (value < 0 || value > 4)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"VerificationLevel must be between 0 and 4, got {value}.");
        Value = value;
    }

    public static readonly VerificationLevel None = new(0);
    public static readonly VerificationLevel Basic = new(1);
    public static readonly VerificationLevel Standard = new(2);
    public static readonly VerificationLevel Enhanced = new(3);
    public static readonly VerificationLevel Full = new(4);

    public override string ToString() => Value switch
    {
        0 => "None",
        1 => "Basic",
        2 => "Standard",
        3 => "Enhanced",
        4 => "Full",
        _ => Value.ToString()
    };
}
