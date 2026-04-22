using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.TrustSystem.Identity.Credential;
using Whycespace.Domain.TrustSystem.Identity.Registry;
using Whycespace.Domain.TrustSystem.Identity.Trust;
using Whycespace.Domain.TrustSystem.Identity.Consent;
using Whycespace.Domain.TrustSystem.Identity.Device;
using Whycespace.Domain.TrustSystem.Identity.Federation;
using Whycespace.Domain.TrustSystem.Access.Session;
using Whycespace.Domain.TrustSystem.Access.Grant;
using Whycespace.Domain.TrustSystem.Access.Request;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.TrustSystem.ReplayLosslessness;

/// <summary>
/// INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — trust-system
/// Verifies that LoadFromHistory produces structurally identical aggregate state
/// to direct factory construction — all VO fields survive the event round-trip.
/// </summary>
public sealed class TrustSystemReplayLosslessnessTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    // ── RegistryAggregate (lockout path) ────────────────────────────────────

    [Fact]
    public void RegistryAggregate_LockedPath_Replay_PreservesAllFields()
    {
        var id = new RegistryId(IdGen.Generate("LS:registry:locked:id"));
        var descriptor = new RegistrationDescriptor("replay@whycespace.com", "Individual");
        var direct = RegistryAggregate.Initiate(id, descriptor, BaseTime);
        direct.LockOut("INV-REPLAY: lockout path regression");

        var replayed = (RegistryAggregate)Activator.CreateInstance(typeof(RegistryAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(id, replayed.RegistryId);
        Assert.Equal(descriptor.Email, replayed.Descriptor.Email);
        Assert.Equal(descriptor.RegistrationType, replayed.Descriptor.RegistrationType);
        Assert.Equal(RegistrationStatus.Locked, replayed.Status);
    }

    // ── CredentialAggregate (hash field) ─────────────────────────────────────

    [Fact]
    public void CredentialAggregate_WithHash_Replay_PreservesHashField()
    {
        var id = new CredentialId(IdGen.Generate("LS:credential:hash:id"));
        var identityRef = IdGen.Generate("LS:credential:hash:identity");
        var hashValue = new CredentialHashValue("$2a$12$longenoughhashabcdefghijklmnopqrstuvwxyz01234567890");
        var descriptor = new CredentialDescriptor(identityRef, "Password", hashValue);

        var direct = CredentialAggregate.Issue(id, descriptor);

        var replayed = (CredentialAggregate)Activator.CreateInstance(typeof(CredentialAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(id, replayed.Id);
        Assert.Equal(descriptor.CredentialType, replayed.Descriptor.CredentialType);
        Assert.Equal(hashValue, replayed.Descriptor.CredentialHash);
        Assert.Equal(CredentialStatus.Issued, replayed.Status);
    }

    // ── TrustAggregate ───────────────────────────────────────────────────────

    [Fact]
    public void TrustAggregate_Replay_PreservesAllVoFields()
    {
        var id = new TrustId(IdGen.Generate("LS:trust:id"));
        var descriptor = new TrustDescriptor(
            IdGen.Generate("LS:trust:identity-ref"), "Behavioural", 0.8m);

        var direct = TrustAggregate.Assess(id, descriptor, BaseTime);
        direct.Activate();

        var replayed = (TrustAggregate)Activator.CreateInstance(typeof(TrustAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.TrustId, replayed.TrustId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Descriptor.Score, replayed.Descriptor.Score);
        Assert.Equal(direct.Descriptor.TrustCategory, replayed.Descriptor.TrustCategory);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── ConsentAggregate ─────────────────────────────────────────────────────

    [Fact]
    public void ConsentAggregate_Replay_PreservesAllVoFields()
    {
        var id = new ConsentId(IdGen.Generate("LS:consent:id"));
        var descriptor = new ConsentDescriptor(
            IdGen.Generate("LS:consent:identity-ref"), "data-processing", "AnalyticsUse");

        var direct = ConsentAggregate.Grant(id, descriptor, BaseTime);

        var replayed = (ConsentAggregate)Activator.CreateInstance(typeof(ConsentAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.ConsentId, replayed.ConsentId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Descriptor.ConsentScope, replayed.Descriptor.ConsentScope);
        Assert.Equal(direct.Descriptor.ConsentPurpose, replayed.Descriptor.ConsentPurpose);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── DeviceAggregate ──────────────────────────────────────────────────────

    [Fact]
    public void DeviceAggregate_Replay_PreservesAllVoFields()
    {
        var id = new DeviceId(IdGen.Generate("LS:device:id"));
        var descriptor = new DeviceDescriptor(
            IdGen.Generate("LS:device:identity-ref"), "Alice-Laptop", "Desktop");

        var direct = DeviceAggregate.Register(id, descriptor, BaseTime);
        direct.Activate();

        var replayed = (DeviceAggregate)Activator.CreateInstance(typeof(DeviceAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.DeviceId, replayed.DeviceId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Descriptor.DeviceName, replayed.Descriptor.DeviceName);
        Assert.Equal(direct.Descriptor.DeviceType, replayed.Descriptor.DeviceType);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── SessionAggregate ─────────────────────────────────────────────────────

    [Fact]
    public void SessionAggregate_Replay_PreservesAllVoFields()
    {
        var id = new SessionId(IdGen.Generate("LS:session:id"));
        var descriptor = new SessionDescriptor(
            IdGen.Generate("LS:session:identity-ref"), "api-gateway");

        var direct = SessionAggregate.Open(id, descriptor, BaseTime);

        var replayed = (SessionAggregate)Activator.CreateInstance(typeof(SessionAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.SessionId, replayed.SessionId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Descriptor.SessionContext, replayed.Descriptor.SessionContext);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── GrantAggregate ───────────────────────────────────────────────────────

    [Fact]
    public void GrantAggregate_Replay_PreservesAllVoFields()
    {
        var id = new GrantId(IdGen.Generate("LS:grant:id"));
        var descriptor = new GrantDescriptor(
            IdGen.Generate("LS:grant:principal-ref"), "content:write", "ResourceAccess");

        var direct = GrantAggregate.Issue(id, descriptor, BaseTime);
        direct.Activate();

        var replayed = (GrantAggregate)Activator.CreateInstance(typeof(GrantAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.GrantId, replayed.GrantId);
        Assert.Equal(direct.Descriptor.PrincipalReference, replayed.Descriptor.PrincipalReference);
        Assert.Equal(direct.Descriptor.GrantScope, replayed.Descriptor.GrantScope);
        Assert.Equal(direct.Descriptor.GrantType, replayed.Descriptor.GrantType);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── RequestAggregate ─────────────────────────────────────────────────────

    [Fact]
    public void RequestAggregate_Replay_PreservesAllVoFields()
    {
        var id = new RequestId(IdGen.Generate("LS:request:id"));
        var descriptor = new RequestDescriptor(
            IdGen.Generate("LS:request:principal-ref"), "ElevatedAccess", "admin-tools");

        var direct = RequestAggregate.Submit(id, descriptor, BaseTime);
        direct.Approve();

        var replayed = (RequestAggregate)Activator.CreateInstance(typeof(RequestAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.RequestId, replayed.RequestId);
        Assert.Equal(direct.Descriptor.PrincipalReference, replayed.Descriptor.PrincipalReference);
        Assert.Equal(direct.Descriptor.RequestType, replayed.Descriptor.RequestType);
        Assert.Equal(direct.Descriptor.RequestScope, replayed.Descriptor.RequestScope);
        Assert.Equal(direct.Status, replayed.Status);
    }

    // ── FederationAggregate ──────────────────────────────────────────────────

    [Fact]
    public void FederationAggregate_Replay_PreservesAllVoFields()
    {
        var id = new FederationId(IdGen.Generate("LS:federation:id"));
        var descriptor = new FederationDescriptor(
            IdGen.Generate("LS:federation:identity-ref"), "okta-enterprise", "SAML");

        var direct = FederationAggregate.Establish(id, descriptor, BaseTime);

        var replayed = (FederationAggregate)Activator.CreateInstance(typeof(FederationAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(direct.DomainEvents);

        Assert.Equal(direct.FederationId, replayed.FederationId);
        Assert.Equal(direct.Descriptor.IdentityReference, replayed.Descriptor.IdentityReference);
        Assert.Equal(direct.Descriptor.FederatedProvider, replayed.Descriptor.FederatedProvider);
        Assert.Equal(direct.Descriptor.FederationType, replayed.Descriptor.FederationType);
        Assert.Equal(direct.Status, replayed.Status);
    }
}
