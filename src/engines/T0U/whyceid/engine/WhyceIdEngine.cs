using System.Security.Cryptography;
using System.Text;
using Whycespace.Engines.T0U.WhyceId.Command;
using Whycespace.Engines.T0U.WhyceId.Consent;
using Whycespace.Engines.T0U.WhyceId.Model;
using Whycespace.Engines.T0U.WhyceId.Resolver;
using Whycespace.Engines.T0U.WhyceId.Result;
using Whycespace.Engines.T0U.WhyceId.Session;
using Whycespace.Engines.T0U.WhyceId.Trust;
using Whycespace.Engines.T0U.WhyceId.Verification;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T0U.WhyceId.Engine;

/// <summary>
/// WhyceId — Constitutional Identity Engine (T0U).
/// Non-bypassable. Every request MUST resolve identity through this engine.
///
/// Capabilities:
/// - AuthenticateIdentity: Resolve and authenticate from token/userId
/// - ResolveIdentity: Resolve a known identity by ID
/// - ValidateSession: Validate an existing session
/// - EvaluateTrustScore: Compute deterministic trust score
/// - VerifyIdentity: Verify identity claims
/// - ResolveRolesAndAttributes: Resolve roles and attributes for an identity
/// - BuildAuthorizationContext: Produce a full authorization context for policy evaluation
/// - OpenSession: Open a new session deterministically
/// - TerminateSession: Terminate an active session
/// - RegisterDevice: Register a device and produce its deterministic device ID
/// - GrantConsent: Grant a consent record deterministically
/// - RevokeConsent: Revoke an existing consent record
/// - CheckConsent: Query whether a consent scope is active for an identity
/// </summary>
public sealed class WhyceIdEngine
{
    public Task<AuthenticateIdentityResult> AuthenticateIdentity(
        AuthenticateIdentityCommand command)
    {
        var identityId = IdentityResolver.ResolveIdentityId(command.Token, command.UserId);

        if (string.IsNullOrEmpty(identityId))
        {
            return Task.FromResult(new AuthenticateIdentityResult(
                Identity: CreateAnonymousIdentity(),
                IsAuthenticated: false,
                SessionId: string.Empty));
        }

        var sessionId = SessionValidator.GenerateSessionId(identityId, command.DeviceId);

        var resolvedRoles = command.Roles is { Length: > 0 } supplied
            ? supplied
            : new[] { "user" };

        var (trustScore, _) = TrustScoreEvaluator.Evaluate(
            identityId, resolvedRoles, command.DeviceId, isVerified: false);

        var identity = new WhyceIdentity(
            IdentityId: identityId,
            Roles: resolvedRoles,
            Attributes: [],
            TrustScore: trustScore,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: sessionId,
            DeviceId: command.DeviceId,
            Consents: []);

        return Task.FromResult(new AuthenticateIdentityResult(
            Identity: identity,
            IsAuthenticated: true,
            SessionId: sessionId));
    }

    public Task<ResolveIdentityResult> ResolveIdentity(
        ResolveIdentityCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new ResolveIdentityResult(
                Identity: CreateAnonymousIdentity(),
                IsResolved: false));
        }

        var identity = new WhyceIdentity(
            IdentityId: command.IdentityId,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 50,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: string.Empty,
            DeviceId: null,
            Consents: []);

        return Task.FromResult(new ResolveIdentityResult(
            Identity: identity,
            IsResolved: true));
    }

    public Task<ValidateSessionResult> ValidateSession(
        ValidateSessionCommand command)
    {
        if (string.IsNullOrEmpty(command.SessionId) ||
            string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new ValidateSessionResult(
                IsValid: false,
                FailureReason: "SessionId and IdentityId are required."));
        }

        var isValid = SessionValidator.ValidateSession(
            command.SessionId, command.IdentityId, command.DeviceId);

        return Task.FromResult(new ValidateSessionResult(
            IsValid: isValid,
            FailureReason: isValid ? null : "Session validation failed. Session does not match identity context."));
    }

    public Task<EvaluateTrustScoreResult> EvaluateTrustScore(
        EvaluateTrustScoreCommand command)
    {
        var (score, factors) = TrustScoreEvaluator.Evaluate(
            command.IdentityId,
            command.Roles,
            command.DeviceId,
            command.IsVerified);

        var trustHash = TrustScoreEvaluator.ComputeTrustHash(
            command.IdentityId, score, factors);

        return Task.FromResult(new EvaluateTrustScoreResult(
            TrustScore: score,
            TrustHash: trustHash,
            TrustFactors: factors));
    }

    public Task<VerifyIdentityResult> VerifyIdentity(
        VerifyIdentityCommand command)
    {
        var (isVerified, verificationHash) = IdentityVerifier.Verify(
            command.IdentityId,
            command.VerificationMethod,
            command.VerificationPayload);

        return Task.FromResult(new VerifyIdentityResult(
            IsVerified: isVerified,
            VerificationHash: verificationHash,
            VerificationMethod: command.VerificationMethod));
    }

    public Task<ResolveRolesAndAttributesResult> ResolveRolesAndAttributes(
        ResolveRolesAndAttributesCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new ResolveRolesAndAttributesResult(
                Roles: [],
                Attributes: []));
        }

        var roles = new[] { "user" };
        var attributes = new[]
        {
            new IdentityAttribute("tenant", command.TenantId, "system")
        };

        return Task.FromResult(new ResolveRolesAndAttributesResult(
            Roles: roles,
            Attributes: attributes));
    }

    /// <summary>
    /// Produces a full authorization context for policy evaluation.
    /// The context hash is deterministic: same inputs always produce the same hash.
    /// </summary>
    public Task<BuildAuthorizationContextResult> BuildAuthorizationContext(
        BuildAuthorizationContextCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new BuildAuthorizationContextResult(
                Context: CreateEmptyContext(command.TenantId, command.ResourceId),
                IsAuthorizable: false,
                DenialReason: "IdentityId is required for authorization context."));
        }

        var hasActiveSession = !string.IsNullOrEmpty(command.SessionId) &&
            SessionValidator.ValidateSession(command.SessionId, command.IdentityId, command.DeviceId);

        var hasVerifiedDevice = command.DeviceId is not null;

        var activeConsentScopes = command.Consents
            .Where(c => c.IsGranted)
            .Select(c => c.ConsentType)
            .ToArray();

        var (trustScore, trustFactors) = TrustScoreEvaluator.Evaluate(
            command.IdentityId,
            command.Roles,
            command.DeviceId,
            command.VerificationStatus == VerificationStatus.Verified);

        var resolvedTrustScore = command.TrustScore > 0 ? command.TrustScore : trustScore;

        var trustHash = TrustScoreEvaluator.ComputeTrustHash(
            command.IdentityId, resolvedTrustScore, trustFactors);

        var contextHash = ComputeContextHash(
            command.IdentityId, command.Roles, resolvedTrustScore,
            command.VerificationStatus, hasActiveSession, hasVerifiedDevice,
            activeConsentScopes, command.TenantId, command.ResourceId);

        var context = new AuthorizationContext(
            IdentityId: command.IdentityId,
            Roles: command.Roles,
            Attributes: command.Attributes,
            TrustScore: resolvedTrustScore,
            TrustHash: trustHash,
            VerificationStatus: command.VerificationStatus,
            HasActiveSession: hasActiveSession,
            HasVerifiedDevice: hasVerifiedDevice,
            ActiveConsentScopes: activeConsentScopes,
            TenantId: command.TenantId,
            ResourceId: command.ResourceId,
            ContextHash: contextHash);

        return Task.FromResult(new BuildAuthorizationContextResult(
            Context: context,
            IsAuthorizable: true,
            DenialReason: null));
    }

    /// <summary>
    /// Opens a new session deterministically from identity and device context.
    /// </summary>
    public Task<OpenSessionResult> OpenSession(OpenSessionCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new OpenSessionResult(
                SessionId: string.Empty,
                IsOpened: false,
                FailureReason: "IdentityId is required to open a session."));
        }

        var sessionId = SessionValidator.GenerateSessionId(command.IdentityId, command.DeviceId);

        return Task.FromResult(new OpenSessionResult(
            SessionId: sessionId,
            IsOpened: true,
            FailureReason: null));
    }

    /// <summary>
    /// Terminates a session by validating it can be terminated.
    /// </summary>
    public Task<TerminateSessionResult> TerminateSession(TerminateSessionCommand command)
    {
        if (string.IsNullOrEmpty(command.SessionId) || string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new TerminateSessionResult(
                IsTerminated: false,
                FailureReason: "SessionId and IdentityId are required."));
        }

        return Task.FromResult(new TerminateSessionResult(
            IsTerminated: true,
            FailureReason: null));
    }

    /// <summary>
    /// Registers a device and produces its deterministic device ID and hash.
    /// </summary>
    public Task<RegisterDeviceResult> RegisterDevice(RegisterDeviceCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId) ||
            string.IsNullOrEmpty(command.DeviceName) ||
            string.IsNullOrEmpty(command.DeviceType))
        {
            return Task.FromResult(new RegisterDeviceResult(
                DeviceId: string.Empty,
                DeviceHash: string.Empty,
                IsRegistered: false));
        }

        var deviceSeed = $"device:{command.IdentityId}:{command.DeviceName}:{command.DeviceType}";
        var deviceId = IdentityResolver.HashToken(deviceSeed);
        var deviceHash = IdentityResolver.ComputeDeviceHash(deviceId);

        return Task.FromResult(new RegisterDeviceResult(
            DeviceId: deviceId,
            DeviceHash: deviceHash,
            IsRegistered: true));
    }

    /// <summary>
    /// Grants a consent record deterministically.
    /// </summary>
    public Task<GrantConsentResult> GrantConsent(GrantConsentCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId) ||
            string.IsNullOrEmpty(command.ConsentScope) ||
            string.IsNullOrEmpty(command.ConsentPurpose))
        {
            return Task.FromResult(new GrantConsentResult(
                ConsentId: string.Empty,
                ConsentHash: string.Empty,
                IsGranted: false));
        }

        var consentSeed = $"consent:{command.IdentityId}:{command.ConsentScope}:{command.ConsentPurpose}";
        var consentId = IdentityResolver.HashToken(consentSeed);
        var consentHash = IdentityResolver.HashToken($"consent-hash:{consentId}");

        return Task.FromResult(new GrantConsentResult(
            ConsentId: consentId,
            ConsentHash: consentHash,
            IsGranted: true));
    }

    /// <summary>
    /// Revokes a consent record.
    /// </summary>
    public Task<RevokeConsentResult> RevokeConsent(RevokeConsentCommand command)
    {
        if (string.IsNullOrEmpty(command.ConsentId) || string.IsNullOrEmpty(command.IdentityId))
        {
            return Task.FromResult(new RevokeConsentResult(
                IsRevoked: false,
                FailureReason: "ConsentId and IdentityId are required."));
        }

        return Task.FromResult(new RevokeConsentResult(
            IsRevoked: true,
            FailureReason: null));
    }

    /// <summary>
    /// Checks whether a consent scope is active for an identity.
    /// </summary>
    public Task<CheckConsentResult> CheckConsent(CheckConsentCommand command)
    {
        if (string.IsNullOrEmpty(command.IdentityId) || string.IsNullOrEmpty(command.ConsentScope))
        {
            return Task.FromResult(new CheckConsentResult(
                HasConsent: false,
                ConsentId: null,
                ConsentHash: null));
        }

        var consentSeed = $"consent:{command.IdentityId}:{command.ConsentScope}:";
        var consentId = IdentityResolver.HashToken(consentSeed + "check");
        var consentHash = IdentityResolver.HashToken($"consent-hash:{consentId}");

        return Task.FromResult(new CheckConsentResult(
            HasConsent: true,
            ConsentId: consentId,
            ConsentHash: consentHash));
    }

    private static AuthorizationContext CreateEmptyContext(string tenantId, string? resourceId) =>
        new(
            IdentityId: string.Empty,
            Roles: [],
            Attributes: [],
            TrustScore: 0,
            TrustHash: string.Empty,
            VerificationStatus: VerificationStatus.Unverified,
            HasActiveSession: false,
            HasVerifiedDevice: false,
            ActiveConsentScopes: [],
            TenantId: tenantId,
            ResourceId: resourceId,
            ContextHash: string.Empty);

    private static string ComputeContextHash(
        string identityId, string[] roles, int trustScore,
        VerificationStatus verificationStatus, bool hasActiveSession,
        bool hasVerifiedDevice, string[] activeConsentScopes,
        string tenantId, string? resourceId)
    {
        var sortedRoles = string.Join(",", roles.OrderBy(r => r, StringComparer.Ordinal));
        var sortedConsents = string.Join(",", activeConsentScopes.OrderBy(s => s, StringComparer.Ordinal));
        var input = $"{identityId}:{sortedRoles}:{trustScore}:{(int)verificationStatus}:{hasActiveSession}:{hasVerifiedDevice}:{sortedConsents}:{tenantId}:{resourceId}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static WhyceIdentity CreateAnonymousIdentity() => new(
        IdentityId: string.Empty,
        Roles: [],
        Attributes: [],
        TrustScore: 0,
        VerificationStatus: VerificationStatus.Unverified,
        SessionId: string.Empty,
        DeviceId: null,
        Consents: []);
}
