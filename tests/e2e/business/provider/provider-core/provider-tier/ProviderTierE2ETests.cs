using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Provider.ProviderCore.ProviderTier;

/// <summary>
/// E2E smoke test for the business/provider/provider-core/provider-tier vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_provider_provider_tier.provider_tier_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProviderTierE2ETests
{
    private const string ProjSchema = "projection_business_provider_provider_tier";
    private const string ProjTable  = "provider_tier_read_model";

    private readonly BusinessE2EFixture _fix;
    public ProviderTierE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var providerTierId = _fix.SeedId("provider-tier:happy:id");
        var corrCreate     = _fix.SeedId("provider-tier:happy:corr:create");
        var corrActivate   = _fix.SeedId("provider-tier:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-core/provider-tier/create",
            new CreateProviderTierRequestModel(
                providerTierId,
                "gold",
                "Gold Tier",
                1),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, providerTierId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-core/provider-tier/activate",
            new ProviderTierIdRequestModel(providerTierId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ProviderTierReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/provider-core/provider-tier/{providerTierId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProviderTierReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(providerTierId, read!.ProviderTierId);
        Assert.Equal("Active", read.Status);
    }
}
