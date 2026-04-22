using Whycespace.Engines.T0U.WhyceId.Command;
using Whycespace.Engines.T0U.WhyceId.Engine;

namespace Whycespace.Tests.Unit.Engine.WhyceId;

public sealed class WhyceIdEngineSessionDeviceConsentTests
{
    private readonly WhyceIdEngine _engine = new();
    private static string SomeIdentityId => "bob-identity-id-canonical";

    // ── Session ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task OpenSession_WithValidIdentity_ReturnsSessionId()
    {
        var result = await _engine.OpenSession(
            new OpenSessionCommand(SomeIdentityId, null, "web-dashboard"));

        Assert.True(result.IsOpened);
        Assert.NotEmpty(result.SessionId);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task OpenSession_SameInputs_ProduceSameSessionId()
    {
        var r1 = await _engine.OpenSession(new OpenSessionCommand(SomeIdentityId, "device-abc", "api"));
        var r2 = await _engine.OpenSession(new OpenSessionCommand(SomeIdentityId, "device-abc", "api"));

        Assert.Equal(r1.SessionId, r2.SessionId);
    }

    [Fact]
    public async Task OpenSession_DifferentDevice_ProducesDifferentSessionId()
    {
        var r1 = await _engine.OpenSession(new OpenSessionCommand(SomeIdentityId, "device-A", "api"));
        var r2 = await _engine.OpenSession(new OpenSessionCommand(SomeIdentityId, "device-B", "api"));

        Assert.NotEqual(r1.SessionId, r2.SessionId);
    }

    [Fact]
    public async Task OpenSession_EmptyIdentity_ReturnsNotOpened()
    {
        var result = await _engine.OpenSession(
            new OpenSessionCommand(string.Empty, null, "api"));

        Assert.False(result.IsOpened);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task OpenSession_SessionIsValidatable()
    {
        var opened = await _engine.OpenSession(
            new OpenSessionCommand(SomeIdentityId, "device-xyz", "api"));

        var validated = await _engine.ValidateSession(
            new ValidateSessionCommand(opened.SessionId, SomeIdentityId, "device-xyz"));

        Assert.True(validated.IsValid);
    }

    [Fact]
    public async Task TerminateSession_WithValidIds_ReturnsTerminated()
    {
        var opened = await _engine.OpenSession(
            new OpenSessionCommand(SomeIdentityId, null, "api"));

        var terminated = await _engine.TerminateSession(
            new TerminateSessionCommand(opened.SessionId, SomeIdentityId));

        Assert.True(terminated.IsTerminated);
        Assert.Null(terminated.FailureReason);
    }

    [Fact]
    public async Task TerminateSession_EmptyIds_ReturnsNotTerminated()
    {
        var result = await _engine.TerminateSession(
            new TerminateSessionCommand(string.Empty, SomeIdentityId));

        Assert.False(result.IsTerminated);
        Assert.NotNull(result.FailureReason);
    }

    // ── Device ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterDevice_WithValidInput_ReturnsDeviceId()
    {
        var result = await _engine.RegisterDevice(
            new RegisterDeviceCommand(SomeIdentityId, "Bob-iPhone", "Mobile"));

        Assert.True(result.IsRegistered);
        Assert.NotEmpty(result.DeviceId);
        Assert.NotEmpty(result.DeviceHash);
    }

    [Fact]
    public async Task RegisterDevice_SameInputs_ProduceSameDeviceId()
    {
        var cmd = new RegisterDeviceCommand(SomeIdentityId, "Bob-Laptop", "Desktop");
        var r1 = await _engine.RegisterDevice(cmd);
        var r2 = await _engine.RegisterDevice(cmd);

        Assert.Equal(r1.DeviceId, r2.DeviceId);
        Assert.Equal(r1.DeviceHash, r2.DeviceHash);
    }

    [Fact]
    public async Task RegisterDevice_DifferentName_ProducesDifferentDeviceId()
    {
        var r1 = await _engine.RegisterDevice(new RegisterDeviceCommand(SomeIdentityId, "DeviceA", "Mobile"));
        var r2 = await _engine.RegisterDevice(new RegisterDeviceCommand(SomeIdentityId, "DeviceB", "Mobile"));

        Assert.NotEqual(r1.DeviceId, r2.DeviceId);
    }

    [Fact]
    public async Task RegisterDevice_EmptyInput_ReturnsNotRegistered()
    {
        var result = await _engine.RegisterDevice(
            new RegisterDeviceCommand(string.Empty, "Device", "Mobile"));

        Assert.False(result.IsRegistered);
    }

    // ── Consent ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GrantConsent_WithValidInput_ReturnsGranted()
    {
        var result = await _engine.GrantConsent(
            new GrantConsentCommand(SomeIdentityId, "marketing-emails", "CampaignOptIn"));

        Assert.True(result.IsGranted);
        Assert.NotEmpty(result.ConsentId);
        Assert.NotEmpty(result.ConsentHash);
    }

    [Fact]
    public async Task GrantConsent_SameInputs_ProduceSameConsentId()
    {
        var cmd = new GrantConsentCommand(SomeIdentityId, "analytics", "PlatformAnalytics");
        var r1 = await _engine.GrantConsent(cmd);
        var r2 = await _engine.GrantConsent(cmd);

        Assert.Equal(r1.ConsentId, r2.ConsentId);
        Assert.Equal(r1.ConsentHash, r2.ConsentHash);
    }

    [Fact]
    public async Task GrantConsent_DifferentScope_ProducesDifferentConsentId()
    {
        var r1 = await _engine.GrantConsent(new GrantConsentCommand(SomeIdentityId, "scope-A", "Purpose"));
        var r2 = await _engine.GrantConsent(new GrantConsentCommand(SomeIdentityId, "scope-B", "Purpose"));

        Assert.NotEqual(r1.ConsentId, r2.ConsentId);
    }

    [Fact]
    public async Task GrantConsent_EmptyInput_ReturnsNotGranted()
    {
        var result = await _engine.GrantConsent(
            new GrantConsentCommand(SomeIdentityId, string.Empty, "Purpose"));

        Assert.False(result.IsGranted);
    }

    [Fact]
    public async Task RevokeConsent_WithValidIds_ReturnsRevoked()
    {
        var granted = await _engine.GrantConsent(
            new GrantConsentCommand(SomeIdentityId, "data-processing", "Analytics"));

        var revoked = await _engine.RevokeConsent(
            new RevokeConsentCommand(granted.ConsentId, SomeIdentityId));

        Assert.True(revoked.IsRevoked);
        Assert.Null(revoked.FailureReason);
    }

    [Fact]
    public async Task RevokeConsent_EmptyIds_ReturnsNotRevoked()
    {
        var result = await _engine.RevokeConsent(
            new RevokeConsentCommand(string.Empty, SomeIdentityId));

        Assert.False(result.IsRevoked);
        Assert.NotNull(result.FailureReason);
    }

    [Fact]
    public async Task CheckConsent_WithValidScope_ReturnsHasConsent()
    {
        var result = await _engine.CheckConsent(
            new CheckConsentCommand(SomeIdentityId, "analytics"));

        Assert.True(result.HasConsent);
        Assert.NotNull(result.ConsentId);
    }

    [Fact]
    public async Task CheckConsent_EmptyScope_ReturnsNoConsent()
    {
        var result = await _engine.CheckConsent(
            new CheckConsentCommand(SomeIdentityId, string.Empty));

        Assert.False(result.HasConsent);
    }
}
