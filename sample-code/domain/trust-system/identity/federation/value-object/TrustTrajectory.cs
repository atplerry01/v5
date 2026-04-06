namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Tracks the direction and stability of an issuer's trust over time.
/// Computed deterministically from current and previous scores.
/// </summary>
public sealed record TrustTrajectory
{
    public decimal CurrentScore { get; }
    public decimal PreviousScore { get; }
    public TrustTrend Trend { get; }
    public decimal Volatility { get; }

    public TrustTrajectory(decimal currentScore, decimal previousScore, decimal volatility)
    {
        if (volatility < 0 || volatility > 1)
            throw new ArgumentOutOfRangeException(nameof(volatility),
                $"Volatility must be between 0 and 1, got {volatility}.");

        CurrentScore = currentScore;
        PreviousScore = previousScore;
        Volatility = volatility;

        var delta = currentScore - previousScore;
        Trend = delta switch
        {
            > 1m => TrustTrend.Improving,
            < -1m => TrustTrend.Degrading,
            _ => TrustTrend.Stable
        };
    }

    public static TrustTrajectory Initial(decimal score) =>
        new(score, score, 0m);

    /// <summary>
    /// Compute volatility from a history of score deltas.
    /// Deterministic: same deltas always produce same volatility.
    /// </summary>
    public static decimal ComputeVolatility(IReadOnlyList<decimal> scoreHistory)
    {
        if (scoreHistory.Count < 2) return 0m;

        var deltas = new List<decimal>();
        for (var i = 1; i < scoreHistory.Count; i++)
            deltas.Add(Math.Abs(scoreHistory[i] - scoreHistory[i - 1]));

        var avgDelta = deltas.Average();
        // Normalize to 0–1 range (max meaningful delta is ~50)
        return Math.Clamp(avgDelta / 50m, 0m, 1m);
    }
}

public enum TrustTrend
{
    Improving,
    Stable,
    Degrading
}
