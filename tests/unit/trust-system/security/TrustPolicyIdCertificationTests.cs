using Whycespace.Shared.Contracts.Trust.Access.Session;
using Whycespace.Shared.Contracts.Trust.Identity.Consent;
using Whycespace.Shared.Contracts.Trust.Identity.Credential;
using Whycespace.Shared.Contracts.Trust.Identity.Profile;
using Whycespace.Shared.Contracts.Trust.Identity.Registry;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Tests.Unit.TrustSystem.Security;

/// <summary>
/// 2.8.21 — Certification tests for trust-system policy bindings.
/// Verifies that every trust command has a declared, non-empty policy ID constant
/// and that all policy IDs follow the canonical naming convention.
/// </summary>
public sealed class TrustPolicyIdCertificationTests
{
    private static void AssertCanonicalPolicyId(string policyId, string expectedPrefix)
    {
        Assert.False(string.IsNullOrWhiteSpace(policyId));
        Assert.StartsWith(expectedPrefix, policyId);
        Assert.DoesNotContain(" ", policyId);
        Assert.Equal(policyId.ToLowerInvariant(), policyId);
    }

    // ── Registry (5) ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RegistryPolicyIds.Initiate)]
    [InlineData(RegistryPolicyIds.Verify)]
    [InlineData(RegistryPolicyIds.Activate)]
    [InlineData(RegistryPolicyIds.Reject)]
    [InlineData(RegistryPolicyIds.Lock)]
    public void RegistryPolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.identity.registry.");

    [Fact]
    public void RegistryPolicyIds_Lock_IsDistinctFromReject()
        => Assert.NotEqual(RegistryPolicyIds.Lock, RegistryPolicyIds.Reject);

    [Fact]
    public void RegistryPolicyIds_AllFive_AreDistinct()
    {
        var ids = new[]
        {
            RegistryPolicyIds.Initiate, RegistryPolicyIds.Verify,
            RegistryPolicyIds.Activate, RegistryPolicyIds.Reject, RegistryPolicyIds.Lock
        };
        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    // ── Profile (3) ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(ProfilePolicyIds.Create)]
    [InlineData(ProfilePolicyIds.Activate)]
    [InlineData(ProfilePolicyIds.Deactivate)]
    public void ProfilePolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.identity.profile.");

    // ── Consent (3) ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(ConsentPolicyIds.Grant)]
    [InlineData(ConsentPolicyIds.Revoke)]
    [InlineData(ConsentPolicyIds.Expire)]
    public void ConsentPolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.identity.consent.");

    // ── Session (3) ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(SessionPolicyIds.Open)]
    [InlineData(SessionPolicyIds.Expire)]
    [InlineData(SessionPolicyIds.Terminate)]
    public void SessionPolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.access.session.");

    // ── Credential (3) ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(CredentialPolicyIds.Issue)]
    [InlineData(CredentialPolicyIds.Activate)]
    [InlineData(CredentialPolicyIds.Revoke)]
    public void CredentialPolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.identity.credential.");

    // ── Verification (3) ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(VerificationPolicyIds.Initiate)]
    [InlineData(VerificationPolicyIds.Pass)]
    [InlineData(VerificationPolicyIds.Fail)]
    public void VerificationPolicyId_IsCanonical(string policyId)
        => AssertCanonicalPolicyId(policyId, "whyce.trust.identity.verification.");

    // ── Coverage count ────────────────────────────────────────────────────────

    [Fact]
    public void TrustSystem_HasExactly20PolicyIds()
    {
        var all = new[]
        {
            RegistryPolicyIds.Initiate, RegistryPolicyIds.Verify, RegistryPolicyIds.Activate,
            RegistryPolicyIds.Reject,   RegistryPolicyIds.Lock,
            ProfilePolicyIds.Create,    ProfilePolicyIds.Activate,  ProfilePolicyIds.Deactivate,
            ConsentPolicyIds.Grant,     ConsentPolicyIds.Revoke,    ConsentPolicyIds.Expire,
            SessionPolicyIds.Open,      SessionPolicyIds.Expire,    SessionPolicyIds.Terminate,
            CredentialPolicyIds.Issue,  CredentialPolicyIds.Activate, CredentialPolicyIds.Revoke,
            VerificationPolicyIds.Initiate, VerificationPolicyIds.Pass, VerificationPolicyIds.Fail
        };
        Assert.Equal(20, all.Length);
        Assert.Equal(20, all.Distinct().Count());
    }
}
