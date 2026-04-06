namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Deterministic math functions for intelligence scoring.
/// Pure functions — no state, no randomness, no external dependencies.
/// </summary>
public static class DecayMath
{
    /// <summary>
    /// Exponential decay: decayFactor = exp(-lambda * ageInDays).
    /// Lambda = 0.05 by default (half-life ~14 days).
    /// Returns value in range (0, 1].
    /// </summary>
    public static decimal ExponentialDecay(double ageInDays, double lambda = 0.05)
    {
        if (ageInDays <= 0) return 1m;
        var decay = Math.Exp(-lambda * ageInDays);
        return (decimal)Math.Max(decay, 0.01); // floor at 1% to never fully ignore
    }

    /// <summary>
    /// Apply time decay to a signal weight.
    /// weight * exp(-lambda * ageInDays)
    /// </summary>
    public static decimal DecayWeight(decimal weight, DateTimeOffset signalTime, DateTimeOffset now, double lambda = 0.05)
    {
        var ageInDays = (now - signalTime).TotalDays;
        return weight * ExponentialDecay(ageInDays, lambda);
    }

    /// <summary>
    /// Non-linear scoring: weight * log(1 + signal).
    /// Provides diminishing returns for high signal values.
    /// </summary>
    public static decimal LogScale(decimal weight, decimal signal)
    {
        if (signal <= 0) return 0m;
        return weight * (decimal)Math.Log((double)(1m + signal));
    }

    /// <summary>
    /// Capped diminishing returns: weight * (1 - exp(-rate * signal)).
    /// Approaches `weight` asymptotically as signal grows.
    /// </summary>
    public static decimal DiminishingReturns(decimal maxContribution, decimal signal, double rate = 0.1)
    {
        if (signal <= 0) return 0m;
        return maxContribution * (1m - (decimal)Math.Exp(-rate * (double)signal));
    }
}
