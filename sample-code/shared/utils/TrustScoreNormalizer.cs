namespace Whycespace.Shared.Utils;

/// <summary>
/// Normalizes trust scores to fixed precision to prevent floating-point drift.
/// Rounds to 4 decimal places using MidpointRounding.ToEven (banker's rounding).
/// Ensures deterministic hashing: same logical trust → same string representation.
/// </summary>
public static class TrustScoreNormalizer
{
    private const int Precision = 4;

    /// <summary>
    /// Normalizes a trust score to fixed precision.
    /// Clamps to [0.0, 1.0] range.
    /// </summary>
    public static double Normalize(double trustScore)
    {
        var clamped = Math.Clamp(trustScore, 0.0, 1.0);
        return Math.Round(clamped, Precision, MidpointRounding.ToEven);
    }

    /// <summary>
    /// Converts a normalized trust score to a deterministic string for hashing.
    /// Always produces the same string for the same logical value.
    /// </summary>
    public static string ToHashString(double trustScore)
    {
        return Normalize(trustScore).ToString($"F{Precision}", System.Globalization.CultureInfo.InvariantCulture);
    }
}
