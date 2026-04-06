namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// T3I Intelligence Engine — evaluates issuer behavior over time.
///
/// FIX 5: Emits issuer.sanction.recommended when:
///   - ReputationScore &lt; SanctionThreshold
///   - OR repeated anomalies exceed threshold
///
/// Stateless. No persistence. Deterministic.
/// Uses engine-local types instead of domain imports.
/// </summary>
public sealed class IssuerReputationEngine
{
    public IssuerReputationResult Evaluate(EvaluateIssuerReputationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var factors = new List<string>();
        decimal score = 100m;

        // Credential validity
        var validityPenalty = (1m - command.CredentialValidityRate) * 30m;
        score -= validityPenalty;
        if (validityPenalty > 5m)
            factors.Add($"Low credential validity rate ({command.CredentialValidityRate:P1})");

        // Revocation rate
        var revocationPenalty = command.RevocationRate * 25m;
        score -= revocationPenalty;
        if (revocationPenalty > 5m)
            factors.Add($"High revocation rate ({command.RevocationRate:P1})");

        // Incident rate
        var incidentPenalty = Math.Min(command.IncidentRate * 20m, 30m);
        score -= incidentPenalty;
        if (command.IncidentCount > 0)
            factors.Add($"{command.IncidentCount} incidents recorded");

        // Trust trajectory
        if (command.TrajectoryTrend == TrustTrends.Degrading)
        {
            score -= 10m;
            factors.Add("Trust trajectory is degrading");
        }

        // Volatility
        var volatilityPenalty = command.TrajectoryVolatility * 15m;
        score -= volatilityPenalty;
        if (command.TrajectoryVolatility > 0.3m)
            factors.Add($"High trust volatility ({command.TrajectoryVolatility:F2})");

        score = Math.Clamp(score, 0m, 100m);

        var status = score switch
        {
            >= 70m => "Trusted",
            >= 40m => "Watchlist",
            _ => "Suspicious"
        };

        // FIX 5: Sanction signal
        IssuerSanctionSignal? sanctionSignal = null;

        if (score < command.SanctionThreshold || command.RepeatedAnomalyCount >= 3)
        {
            var severity = score < 15m ? "Critical" : score < command.SanctionThreshold ? "High" : "Medium";
            var reason = score < command.SanctionThreshold
                ? $"Reputation score {score:F1} is below sanction threshold {command.SanctionThreshold}"
                : $"Repeated anomalies ({command.RepeatedAnomalyCount}) exceed threshold";

            sanctionSignal = new IssuerSanctionSignal(
                command.IssuerId,
                severity,
                reason,
                score,
                command.RepeatedAnomalyCount);

            factors.Add($"SANCTION RECOMMENDED: {reason}");
        }

        return new IssuerReputationResult(
            command.IssuerId, score, status, factors, sanctionSignal);
    }
}
