using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Provider.ProviderCore.ProviderCapability;

/// <summary>
/// E2E smoke test for the business/provider/provider-core/provider-capability vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_provider_provider_capability.provider_capability_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProviderCapabilityE2ETests
{
    private const string ProjSchema = "projection_business_provider_provider_capability";
    private const string ProjTable  = "provider_capability_read_model";

    private readonly BusinessE2EFixture _fix;
    public ProviderCapabilityE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var providerCapabilityId = _fix.SeedId("provider-capability:happy:id");
        var providerId           = _fix.SeedId("provider-capability:happy:provider");
        var corrCreate           = _fix.SeedId("provider-capability:happy:corr:create");
        var corrActivate         = _fix.SeedId("provider-capability:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-core/provider-capability/create",
            new CreateProviderCapabilityRequestModel(
                providerCapabilityId,
                providerId,
                "compute",
                "Compute Capability"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, providerCapabilityId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-core/provider-capability/activate",
            new ProviderCapabilityIdRequestModel(providerCapabilityId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ProviderCapabilityReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/provider-core/provider-capability/{providerCapabilityId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProviderCapabilityReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(providerCapabilityId, read!.ProviderCapabilityId);
        Assert.Equal("Active", read.Status);
    }
}
