namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Classification of a federation issuer.
/// </summary>
public sealed record IssuerType
{
    public string Value { get; }

    private IssuerType(string value) => Value = value;

    public static readonly IssuerType Government = new("Government");
    public static readonly IssuerType Financial = new("Financial");
    public static readonly IssuerType Enterprise = new("Enterprise");
    public static readonly IssuerType Platform = new("Platform");

    public static IssuerType From(string value)
    {
        Guard.AgainstEmpty(value);
        return value switch
        {
            "Government" => Government,
            "Financial" => Financial,
            "Enterprise" => Enterprise,
            "Platform" => Platform,
            _ => throw new ArgumentException($"Unknown IssuerType: '{value}'.")
        };
    }

    public override string ToString() => Value;
}
