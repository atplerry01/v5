namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// T3I Intelligence Engine — advanced cross-issuer anomaly detection.
///
/// Detects:
///   1. Cross-issuer duplicate external identities
///   2. Conflicting verification levels
///   3. Abnormal trust jumps
///   4. High issuer volatility
///   5. Identity hopping (rapid link/unlink across issuers)
///   6. Trust laundering (low-trust → high-trust issuer boost)
///   7. Coordinated issuer abuse (correlated anomalies)
///
/// Stateless. No persistence. Deterministic.
/// </summary>
public sealed class FederationRiskEngine
{
    public FederationRiskResult Detect(DetectFederationRiskCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var flags = new List<FederationRiskFlag>();

        // 1. Same external identity linked to multiple issuers
        var externalIdGroups = command.ActiveLinks
            .GroupBy(l => l.ExternalId)
            .Where(g => g.Select(l => l.IssuerId).Distinct().Count() > 1)
            .ToList();

        foreach (var group in externalIdGroups)
        {
            var issuerCount = group.Select(l => l.IssuerId).Distinct().Count();
            flags.Add(new FederationRiskFlag(
                "cross_issuer_duplicate",
                $"External identity '{group.Key}' linked across {issuerCount} issuers",
                0.8m));
        }

        // 2. Conflicting verification levels
        var verificationGroups = command.ActiveLinks
            .GroupBy(l => l.ExternalId)
            .Where(g => g.Count() > 1);

        foreach (var group in verificationGroups)
        {
            var levels = group.Select(l => l.VerificationLevel).Distinct().ToList();
            if (levels.Count > 1 && levels.Max() - levels.Min() >= 2)
            {
                flags.Add(new FederationRiskFlag(
                    "conflicting_verification",
                    $"External identity '{group.Key}' has conflicting verification levels (range: {levels.Max() - levels.Min()})",
                    0.6m));
            }
        }

        // 3. Abnormal trust jumps
        foreach (var trajectory in command.IssuerTrajectories)
        {
            var delta = Math.Abs(trajectory.CurrentScore - trajectory.PreviousScore);
            if (delta > 30m)
            {
                flags.Add(new FederationRiskFlag(
                    "abnormal_trust_jump",
                    $"Issuer '{trajectory.IssuerId}' trust jumped {delta:F1} points",
                    0.7m));
            }

            // 4. High volatility
            if (trajectory.Volatility > 0.5m)
            {
                flags.Add(new FederationRiskFlag(
                    "high_issuer_volatility",
                    $"Issuer '{trajectory.IssuerId}' has high trust volatility ({trajectory.Volatility:F2})",
                    0.5m));
            }
        }

        // 5. Identity hopping — rapid link/unlink across issuers
        if (command.LinkActivity is { Count: > 0 })
        {
            var identityActions = command.LinkActivity
                .GroupBy(a => a.IdentityId)
                .ToList();

            foreach (var group in identityActions)
            {
                var distinctIssuers = group.Select(a => a.IssuerId).Distinct().Count();
                var actionCount = group.Count();

                if (actionCount > 4 && distinctIssuers > 2)
                {
                    flags.Add(new FederationRiskFlag(
                        "identity_hopping",
                        $"Identity '{group.Key}' performed {actionCount} link/unlink actions across {distinctIssuers} issuers",
                        0.85m));
                }
            }
        }

        // 6. Trust laundering — low-confidence link coexisting with high-confidence link
        if (command.ActiveLinks.Count > 1)
        {
            var linksByIdentity = command.ActiveLinks
                .GroupBy(l => l.ExternalId)
                .Where(g => g.Count() > 1);

            foreach (var group in linksByIdentity)
            {
                var minConf = group.Min(l => l.Confidence);
                var maxConf = group.Max(l => l.Confidence);

                if (minConf < 0.3m && maxConf > 0.7m)
                {
                    flags.Add(new FederationRiskFlag(
                        "trust_laundering",
                        $"External identity '{group.Key}' has confidence spread {minConf:F2}–{maxConf:F2} — potential trust laundering",
                        0.75m));
                }
            }
        }

        // 7. Coordinated issuer abuse — multiple issuers with correlated degrading trajectories
        var degradingIssuers = command.IssuerTrajectories
            .Where(t => t.Trend == "Degrading")
            .ToList();

        if (degradingIssuers.Count >= 3)
        {
            flags.Add(new FederationRiskFlag(
                "coordinated_issuer_abuse",
                $"{degradingIssuers.Count} issuers showing correlated degrading trajectories",
                0.7m));
        }

        return new FederationRiskResult(
            command.IdentityId,
            flags.Count > 0,
            flags);
    }
}
