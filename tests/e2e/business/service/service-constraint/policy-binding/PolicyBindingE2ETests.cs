using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.PolicyBinding;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceConstraint.PolicyBinding;

/// <summary>
/// E2E smoke test for the business/service/service-constraint/policy-binding vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_policy_binding.policy_binding_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class PolicyBindingE2ETests
{
    private const string ProjSchema = "projection_business_service_policy_binding";
    private const string ProjTable  = "policy_binding_read_model";

    private readonly BusinessE2EFixture _fix;
    public PolicyBindingE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateBind_UpdatesProjection_GetReturnsBound()
    {
        var policyBindingId     = _fix.SeedId("policy-binding:happy:id");
        var serviceDefinitionId = _fix.SeedId("policy-binding:happy:service-definition");
        var corrCreate          = _fix.SeedId("policy-binding:happy:corr:create");
        var corrBind            = _fix.SeedId("policy-binding:happy:corr:bind");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/policy-binding/create",
            new CreatePolicyBindingRequestModel(
                policyBindingId,
                serviceDefinitionId,
                "policy://whyce/default",
                1),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, policyBindingId, BusinessE2EConfig.PollTimeout);

        // 2) Bind
        var bindResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/policy-binding/bind",
            new PolicyBindingIdRequestModel(policyBindingId),
            corrBind);
        Assert.Equal(HttpStatusCode.OK, bindResp.StatusCode);
        var bindAck = await ApiEnvelope.ReadAsync<CommandAck>(bindResp);
        Assert.NotNull(bindAck);
        Assert.True(bindAck!.Success);

        // 3) GET projection — poll until Status == "Bound".
        PolicyBindingReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-constraint/policy-binding/{policyBindingId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<PolicyBindingReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Bound") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(policyBindingId, read!.PolicyBindingId);
        Assert.Equal(serviceDefinitionId, read.ServiceDefinitionId);
        Assert.Equal("Bound", read.Status);
    }
}
