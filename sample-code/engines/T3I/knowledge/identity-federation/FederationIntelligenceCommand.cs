namespace Whycespace.Engines.T3I.IdentityFederation;

/// <summary>
/// Commands for T3I federation intelligence engines.
/// All engines are stateless, read-only, and produce advisory outputs.
/// ALL inputs explicit — no hardcoded values.
/// Uses engine-local types instead of domain imports.
/// </summary>
public abstract record FederationIntelligenceCommand;

public sealed record EvaluateIssuerReputationCommand(
    Guid IssuerId,
    decimal CredentialValidityRate,
    decimal RevocationRate,
    decimal IncidentRate,
    int IncidentCount,
    string TrajectoryTrend,
    decimal TrajectoryVolatility,
    int RepeatedAnomalyCount = 0,
    decimal SanctionThreshold = 30m) : FederationIntelligenceCommand;

public sealed record NormalizeTrustCommand(
    Guid IssuerId,
    decimal RawTrustScore,
    string IssuerType,
    decimal IssuerReputationScore,
    string TrajectoryTrend,
    decimal TrajectoryVolatility) : FederationIntelligenceCommand;

public sealed record DetectFederationRiskCommand(
    Guid IdentityId,
    IReadOnlyList<FederationLinkDto> ActiveLinks,
    IReadOnlyList<TrustTrajectoryDto> IssuerTrajectories,
    IReadOnlyList<LinkActivityDto>? LinkActivity = null) : FederationIntelligenceCommand;

// -- DTOs --

public sealed record FederationLinkDto(
    string ExternalId,
    Guid IssuerId,
    string IssuerType,
    decimal Confidence,
    int VerificationLevel);

public sealed record TrustTrajectoryDto(
    Guid IssuerId,
    decimal CurrentScore,
    decimal PreviousScore,
    string Trend,
    decimal Volatility);

public sealed record LinkActivityDto(
    Guid IdentityId,
    Guid IssuerId,
    string Action,
    DateTimeOffset Timestamp);

/// <summary>
/// Engine-local trust trend constants — decoupled from domain TrustTrend.
/// </summary>
public static class TrustTrends
{
    public const string Degrading = "Degrading";
    public const string Stable = "Stable";
    public const string Improving = "Improving";
}
