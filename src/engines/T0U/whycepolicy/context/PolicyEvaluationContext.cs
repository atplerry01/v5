namespace Whyce.Engines.T0U.WhycePolicy.Context;

/// <summary>
/// Immutable context for policy evaluation.
/// Captures all inputs needed for deterministic policy decisions.
/// </summary>
public sealed record PolicyEvaluationContext(
    string PolicyName,
    string IdentityId,
    string[] Roles,
    int TrustScore,
    string CommandType,
    string TenantId,
    string? ResourceId);
