using Whycespace.Platform.Api.Core.Contracts;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// WhyceID adapter contract for the platform layer.
/// Resolves identity from token/session via WhyceID (T0U external system).
/// Platform never implements identity logic — this adapter delegates to T0U via runtime.
///
/// Responsibilities:
/// - Resolve identity from authenticated token
/// - Fetch full identity profile (Roles, Attributes, TrustScore, Consent)
/// - Validate session state
///
/// NOT responsible for:
/// - Token generation or verification (handled by AuthenticationMiddleware → runtime)
/// - Identity storage or mutation
/// - Policy enforcement (handled by runtime WhycePolicy)
/// </summary>
public interface IWhyceIdService
{
    /// <summary>
    /// Resolves a full WhyceIdentity from an authenticated WhyceId token.
    /// The token has already been validated by AuthenticationMiddleware.
    /// This call fetches the identity profile (roles, attributes, trust, consent).
    /// </summary>
    Task<WhyceIdentity?> ResolveAsync(string whyceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the identity's session is still active and not revoked.
    /// Called after identity resolution to ensure session integrity.
    /// </summary>
    Task<bool> ValidateSessionAsync(Guid identityId, CancellationToken cancellationToken = default);
}
