using System.Globalization;

namespace Whycespace.Shared.Utils;

/// <summary>
/// Normalizes monetary amounts to fixed precision for deterministic hashing.
/// Prevents floating-point/decimal drift in chain evidence.
/// Default: 2 decimal places. Crypto assets: 8 decimal places.
/// </summary>
public static class AmountNormalizer
{
    private const int DefaultPrecision = 2;
    private const int CryptoPrecision = 8;

    private static readonly HashSet<string> CryptoCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BTC", "ETH", "USDT", "USDC", "SOL", "MATIC"
    };

    /// <summary>
    /// Normalizes an amount to fixed precision based on currency.
    /// </summary>
    public static decimal Normalize(decimal amount, string currency)
    {
        var precision = CryptoCurrencies.Contains(currency) ? CryptoPrecision : DefaultPrecision;
        return Math.Round(amount, precision, MidpointRounding.ToEven);
    }

    /// <summary>
    /// Converts a normalized amount to a deterministic string for hashing.
    /// Always produces the same string for the same logical value.
    /// </summary>
    public static string ToHashString(decimal amount, string currency)
    {
        var precision = CryptoCurrencies.Contains(currency) ? CryptoPrecision : DefaultPrecision;
        var normalized = Math.Round(amount, precision, MidpointRounding.ToEven);
        return normalized.ToString($"F{precision}", CultureInfo.InvariantCulture);
    }
}
