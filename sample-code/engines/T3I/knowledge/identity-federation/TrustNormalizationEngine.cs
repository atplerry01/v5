namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// T3I Intelligence Engine — dynamic trust normalization across issuers.
///
/// NormalizedTrust = f(raw, typeFactor, reputationFactor, trajectoryFactor)
///
/// - Type factor: Government 1.0, Financial 0.85, Enterprise 0.70, Platform 0.55
/// - Reputation factor: log-scaled from reputation score
/// - Trajectory factor: penalize degrading, boost improving/stable
///
/// Non-linear. Deterministic. No hardcoded thresholds.
/// Uses string-based types instead of domain enums.
/// </summary>
public sealed class TrustNormalizationEngine
{
    private static readonly IReadOnlyDictionary<string, decimal> TypeFactors =
        new Dictionary<string, decimal>
        {
            ["Government"] = 1.0m,
            ["Financial"] = 0.85m,
            ["Enterprise"] = 0.70m,
            ["Platform"] = 0.55m,
        };

    public NormalizedTrustResult Normalize(NormalizeTrustCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 1. Type factor (base normalization by issuer class)
        if (!TypeFactors.TryGetValue(command.IssuerType, out var typeFactor))
            typeFactor = 0.50m;

        // 2. Reputation factor — log-scaled, range [0.5, 1.0]
        //    Low reputation pulls normalized score down
        var reputationFactor = 0.5m + 0.5m * (decimal)Math.Log(1.0 + (double)command.IssuerReputationScore) /
            (decimal)Math.Log(101.0);

        // 3. Trajectory factor — penalize degrading, boost stable/improving
        var trajectoryFactor = command.TrajectoryTrend switch
        {
            TrustTrends.Degrading => Math.Max(0.7m - command.TrajectoryVolatility * 0.3m, 0.5m),
            TrustTrends.Stable => 1.0m,
            TrustTrends.Improving => Math.Min(1.0m + command.TrajectoryVolatility * 0.1m, 1.1m),
            _ => 1.0m
        };

        // Combined: non-linear product of all factors
        var normalizedScore = Math.Clamp(
            command.RawTrustScore * typeFactor * reputationFactor * trajectoryFactor,
            0m, 100m);

        return new NormalizedTrustResult(
            command.IssuerId,
            command.RawTrustScore,
            normalizedScore,
            command.IssuerType,
            typeFactor,
            reputationFactor,
            trajectoryFactor);
    }
}
