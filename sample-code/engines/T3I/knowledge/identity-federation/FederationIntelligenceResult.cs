namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// Results from T3I federation intelligence engines.
/// Advisory only — never mutates domain state.
/// </summary>
public sealed record IssuerReputationResult(
    Guid IssuerId,
    decimal ReputationScore,
    string ReputationStatus,
    IReadOnlyList<string> Factors,
    IssuerSanctionSignal? SanctionSignal = null);

public sealed record IssuerSanctionSignal(
    Guid IssuerId,
    string Severity,
    string Reason,
    decimal ReputationScore,
    int RepeatedAnomalies);

public sealed record NormalizedTrustResult(
    Guid IssuerId,
    decimal RawScore,
    decimal NormalizedScore,
    string IssuerType,
    decimal TypeFactor,
    decimal ReputationFactor,
    decimal TrajectoryFactor);

public sealed record FederationRiskResult(
    Guid IdentityId,
    bool RiskDetected,
    IReadOnlyList<FederationRiskFlag> Flags);

public sealed record FederationRiskFlag(
    string RiskType,
    string Description,
    decimal Confidence);

/// <summary>
/// Trust propagation guard result — controls how federated trust
/// influences identity trust (E9 integration).
/// </summary>
public sealed record TrustPropagationResult
{
    public required bool PropagationAllowed { get; init; }
    public required decimal CappedContribution { get; init; }
    public string? BlockReason { get; init; }

    public static TrustPropagationResult Allowed(decimal contribution) =>
        new() { PropagationAllowed = true, CappedContribution = contribution };

    public static TrustPropagationResult Blocked(string reason) =>
        new() { PropagationAllowed = false, CappedContribution = 0m, BlockReason = reason };
}
