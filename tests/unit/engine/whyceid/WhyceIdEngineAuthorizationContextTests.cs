using Whycespace.Engines.T0U.WhyceId.Command;
using Whycespace.Engines.T0U.WhyceId.Consent;
using Whycespace.Engines.T0U.WhyceId.Engine;
using Whycespace.Engines.T0U.WhyceId.Model;
using Whycespace.Engines.T0U.WhyceId.Verification;

namespace Whycespace.Tests.Unit.Engine.WhyceId;

public sealed class WhyceIdEngineAuthorizationContextTests
{
    private readonly WhyceIdEngine _engine = new();

    private static string SomeIdentityId => "alice-identity-id-canonical";
    private static string SomeTenantId => "tenant-whycespace";

    [Fact]
    public async Task BuildAuthorizationContext_WithValidIdentity_ReturnsIsAuthorizable()
    {
        var command = new BuildAuthorizationContextCommand(
            IdentityId: SomeIdentityId,
            Roles: ["content-creator", "user"],
            Attributes: [new IdentityAttribute("tier", "premium", "system")],
            TrustScore: 75,
            VerificationStatus: VerificationStatus.Verified,
            SessionId: null,
            DeviceId: null,
            Consents: [],
            TenantId: SomeTenantId,
            ResourceId: null);

        var result = await _engine.BuildAuthorizationContext(command);

        Assert.True(result.IsAuthorizable);
        Assert.Null(result.DenialReason);
        Assert.Equal(SomeIdentityId, result.Context.IdentityId);
        Assert.Equal(SomeTenantId, result.Context.TenantId);
    }

    [Fact]
    public async Task BuildAuthorizationContext_EmptyIdentityId_ReturnsNotAuthorizable()
    {
        var command = new BuildAuthorizationContextCommand(
            IdentityId: string.Empty,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 50,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: null,
            DeviceId: null,
            Consents: [],
            TenantId: SomeTenantId,
            ResourceId: null);

        var result = await _engine.BuildAuthorizationContext(command);

        Assert.False(result.IsAuthorizable);
        Assert.NotNull(result.DenialReason);
    }

    [Fact]
    public async Task BuildAuthorizationContext_WithActiveSession_SetsHasActiveSession()
    {
        var openResult = await _engine.OpenSession(new OpenSessionCommand(SomeIdentityId, null, "api"));

        var command = new BuildAuthorizationContextCommand(
            IdentityId: SomeIdentityId,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 50,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: openResult.SessionId,
            DeviceId: null,
            Consents: [],
            TenantId: SomeTenantId,
            ResourceId: null);

        var result = await _engine.BuildAuthorizationContext(command);

        Assert.True(result.Context.HasActiveSession);
    }

    [Fact]
    public async Task BuildAuthorizationContext_WithGrantedConsents_SetsActiveConsentScopes()
    {
        var grantResult = await _engine.GrantConsent(
            new GrantConsentCommand(SomeIdentityId, "data-processing", "Analytics"));

        var consentRecord = new ConsentRecord(
            ConsentId: grantResult.ConsentId,
            ConsentType: "data-processing",
            IdentityId: SomeIdentityId,
            IsGranted: true,
            ConsentHash: grantResult.ConsentHash);

        var command = new BuildAuthorizationContextCommand(
            IdentityId: SomeIdentityId,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 50,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: null,
            DeviceId: null,
            Consents: [consentRecord],
            TenantId: SomeTenantId,
            ResourceId: null);

        var result = await _engine.BuildAuthorizationContext(command);

        Assert.Contains("data-processing", result.Context.ActiveConsentScopes);
    }

    [Fact]
    public async Task BuildAuthorizationContext_SameInputs_ProduceSameContextHash()
    {
        var command = new BuildAuthorizationContextCommand(
            IdentityId: SomeIdentityId,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 60,
            VerificationStatus: VerificationStatus.Verified,
            SessionId: null,
            DeviceId: null,
            Consents: [],
            TenantId: SomeTenantId,
            ResourceId: "resource-abc");

        var r1 = await _engine.BuildAuthorizationContext(command);
        var r2 = await _engine.BuildAuthorizationContext(command);

        Assert.Equal(r1.Context.ContextHash, r2.Context.ContextHash);
        Assert.NotEmpty(r1.Context.ContextHash);
    }

    [Fact]
    public async Task BuildAuthorizationContext_DifferentRoles_ProduceDifferentContextHash()
    {
        var base_cmd = new BuildAuthorizationContextCommand(
            IdentityId: SomeIdentityId,
            Roles: ["user"],
            Attributes: [],
            TrustScore: 60,
            VerificationStatus: VerificationStatus.Unverified,
            SessionId: null,
            DeviceId: null,
            Consents: [],
            TenantId: SomeTenantId,
            ResourceId: null);

        var elevated_cmd = base_cmd with { Roles = ["admin", "user"] };

        var r1 = await _engine.BuildAuthorizationContext(base_cmd);
        var r2 = await _engine.BuildAuthorizationContext(elevated_cmd);

        Assert.NotEqual(r1.Context.ContextHash, r2.Context.ContextHash);
    }

    [Fact]
    public async Task BuildAuthorizationContext_VerifiedVsUnverified_ProduceDifferentContextHash()
    {
        var unverified = new BuildAuthorizationContextCommand(
            SomeIdentityId, ["user"], [], 50, VerificationStatus.Unverified,
            null, null, [], SomeTenantId, null);

        var verified = unverified with { VerificationStatus = VerificationStatus.Verified };

        var r1 = await _engine.BuildAuthorizationContext(unverified);
        var r2 = await _engine.BuildAuthorizationContext(verified);

        Assert.NotEqual(r1.Context.ContextHash, r2.Context.ContextHash);
    }
}
