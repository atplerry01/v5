using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T3I.Identity;

/// <summary>
/// T3I engine: detects trust anomalies in identity-bound policy decisions.
/// Stateless, deterministic. No persistence, no HTTP, no system clock.
///
/// Anomalies detected:
///   - Low trust + allow decision
///   - Role mismatch (privileged action without required role)
///   - Unverified identity performing privileged action
/// </summary>
public sealed class IdentityTrustAnalyzer : IEngine<AnalyzeIdentityTrustCommand>
{
    private const double LowTrustThreshold = 0.3;
    private static readonly HashSet<string> PrivilegedActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "delete", "admin", "transfer", "governance", "capital.allocate",
        "policy.override", "identity.revoke", "chain.recovery"
    };

    public Task<EngineResult> ExecuteAsync(
        AnalyzeIdentityTrustCommand command,
        EngineContext context,
        CancellationToken cancellationToken = default)
    {
        var anomalies = new List<TrustAnomaly>();
        var normalizedTrust = TrustScoreNormalizer.Normalize(command.TrustScore);
        var isPrivilegedAction = PrivilegedActions.Contains(command.Action)
            || command.Action.StartsWith("admin.", StringComparison.OrdinalIgnoreCase);

        // Anomaly 1: Low trust + allow decision
        if (normalizedTrust < LowTrustThreshold
            && command.Decision.Equals("ALLOW", StringComparison.OrdinalIgnoreCase))
        {
            anomalies.Add(new TrustAnomaly(
                "low_trust_allow",
                $"Low trust ({normalizedTrust:F4}) identity allowed for action '{command.Action}'",
                TrustAnomalySeverity.High));
        }

        // Anomaly 2: Unverified identity performing privileged action
        if (!command.IsVerified && isPrivilegedAction)
        {
            anomalies.Add(new TrustAnomaly(
                "unverified_privileged",
                $"Unverified identity performing privileged action '{command.Action}'",
                TrustAnomalySeverity.Critical));
        }

        // Anomaly 3: Role mismatch — privileged action without admin/operator role
        if (isPrivilegedAction && !HasPrivilegedRole(command.Roles))
        {
            anomalies.Add(new TrustAnomaly(
                "role_mismatch",
                $"Privileged action '{command.Action}' performed without privileged role. Roles: [{string.Join(", ", command.Roles)}]",
                TrustAnomalySeverity.Medium));
        }

        var result = new IdentityTrustAnalysisResult(
            SubjectId: command.SubjectId,
            TrustScore: normalizedTrust,
            AnomalyCount: anomalies.Count,
            Anomalies: anomalies,
            IsClean: anomalies.Count == 0);

        return Task.FromResult(EngineResult.Ok(result));
    }

    private static bool HasPrivilegedRole(string[] roles)
    {
        return roles.Any(r =>
            r.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("operator", StringComparison.OrdinalIgnoreCase) ||
            r.Equals("governance", StringComparison.OrdinalIgnoreCase) ||
            r.StartsWith("admin.", StringComparison.OrdinalIgnoreCase));
    }
}

public sealed record AnalyzeIdentityTrustCommand(
    string SubjectId,
    string Action,
    string Decision,
    string[] Roles,
    double TrustScore,
    bool IsVerified);

public sealed record IdentityTrustAnalysisResult(
    string SubjectId,
    double TrustScore,
    int AnomalyCount,
    List<TrustAnomaly> Anomalies,
    bool IsClean);

public sealed record TrustAnomaly(
    string Type,
    string Description,
    TrustAnomalySeverity Severity);

public enum TrustAnomalySeverity
{
    Low,
    Medium,
    High,
    Critical
}
