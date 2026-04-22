using Whycespace.Engines.T0U.WhyceId.Consent;
using Whycespace.Engines.T0U.WhyceId.Verification;

namespace Whycespace.Engines.T0U.WhyceId.Model;

/// <summary>
/// Full authorization context produced by WhyceID engine.
/// Consumed by WhycePolicy and runtime middleware for access decisions.
/// Immutable and deterministically hashed.
/// </summary>
public sealed record AuthorizationContext(
    string IdentityId,
    string[] Roles,
    IdentityAttribute[] Attributes,
    int TrustScore,
    string TrustHash,
    VerificationStatus VerificationStatus,
    bool HasActiveSession,
    bool HasVerifiedDevice,
    string[] ActiveConsentScopes,
    string TenantId,
    string? ResourceId,
    string ContextHash);
