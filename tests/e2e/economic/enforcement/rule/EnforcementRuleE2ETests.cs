using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Enforcement.Rule;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Enforcement.Rule;
using Whycespace.Tests.E2E.Economic.Enforcement.Setup;

namespace Whycespace.Tests.E2E.Economic.Enforcement.Rule;

/// <summary>
/// End-to-end validation of the enforcement/rule context. Exercises the full
/// Define → Activate → Disable → Retire lifecycle through the real runtime.
/// Invariants covered:
///   • rule state machine: Draft → Active ↔ Disabled → Retired (terminal)
///   • activating a non-existent rule is rejected
///   • projection converges to aggregate state at every transition
/// </summary>
[Collection(EnforcementE2ECollection.Name)]
public sealed class EnforcementRuleE2ETests
{
    private const string ProjSchema = "projection_economic_enforcement_rule";
    private const string ProjTable  = "enforcement_rule_read_model";

    private readonly EnforcementE2EFixture _fix;
    public EnforcementRuleE2ETests(EnforcementE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task Lifecycle_DefineActivateDisableRetire_ProjectionTracksEveryTransition()
    {
        var ruleCode = _fix.RunRuleCode("rule:lifecycle");
        var expectedRuleId = _fix.IdGenerator.Generate($"economic:enforcement:rule:{ruleCode}");
        var correlationId = _fix.SeedId("rule:lifecycle:corr");

        // 1) Define — enters Draft.
        var define = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/define",
            new DefineEnforcementRuleRequestModel(
                ruleCode,
                "Lifecycle Test Rule",
                "Compliance",
                "All",
                "High",
                "E2E lifecycle verification rule."),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, define.StatusCode);

        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Draft", EnforcementE2EConfig.PollTimeout);

        // 2) Activate — Draft → Active.
        var activate = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/activate",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        Assert.Equal(HttpStatusCode.OK, activate.StatusCode);

        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Active", EnforcementE2EConfig.PollTimeout);

        // 3) Disable — Active → Disabled (reversible by re-activation).
        var disable = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/disable",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);

        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Disabled", EnforcementE2EConfig.PollTimeout);

        // 4) Retire — Disabled → Retired (terminal).
        var retire = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/retire",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        Assert.Equal(HttpStatusCode.OK, retire.StatusCode);

        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Retired", EnforcementE2EConfig.PollTimeout);

        // 5) GET — projection reflects the canonical rule payload.
        var get = await _fix.Http.GetAsync($"/api/enforcement/rule/{expectedRuleId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await EnforcementApiEnvelope.ReadAsync<EnforcementRuleReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedRuleId, read.Data!.RuleId);
        Assert.Equal(ruleCode,        read.Data.RuleCode);
        Assert.Equal("Retired",       read.Data.Status);
    }

    [Fact]
    public async Task Invariant_RetireRuleThenActivate_Rejected()
    {
        var ruleCode = _fix.RunRuleCode("rule:invariant:retired");
        var expectedRuleId = _fix.IdGenerator.Generate($"economic:enforcement:rule:{ruleCode}");
        var correlationId = _fix.SeedId("rule:invariant:retired:corr");

        await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/define",
            new DefineEnforcementRuleRequestModel(
                ruleCode, "Invariant Test Rule", "Compliance", "All", "Medium", "Test rule."),
            correlationId);
        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Draft", EnforcementE2EConfig.PollTimeout);

        // Activate, then retire via disable → retire.
        await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/activate",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Active", EnforcementE2EConfig.PollTimeout);

        await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/disable",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Disabled", EnforcementE2EConfig.PollTimeout);

        await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/retire",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        await EnforcementProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedRuleId, "Retired", EnforcementE2EConfig.PollTimeout);

        // Aggregate invariant: Retired is terminal; reactivation must fail.
        var attempt = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/activate",
            new EnforcementRuleIdRequestModel(expectedRuleId), correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, attempt.StatusCode);
    }

    [Fact]
    public async Task Failure_ActivateUnknownRule_ReturnsError_NoProjectionRow()
    {
        var ghostId = _fix.SeedId("rule:fail:ghost");
        var correlationId = _fix.SeedId("rule:fail:corr");

        var activate = await EnforcementApiEnvelope.PostAsync(
            _fix.Http, "/api/enforcement/rule/activate",
            new EnforcementRuleIdRequestModel(ghostId), correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, activate.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/enforcement/rule/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await EnforcementProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
