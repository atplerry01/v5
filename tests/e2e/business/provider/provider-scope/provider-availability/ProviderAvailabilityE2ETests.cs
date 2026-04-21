using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Provider.ProviderScope.ProviderAvailability;

/// <summary>
/// E2E smoke test for the business/provider/provider-scope/provider-availability
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_provider_provider_availability.provider_availability_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProviderAvailabilityE2ETests
{
    private const string ProjSchema = "projection_business_provider_provider_availability";
    private const string ProjTable  = "provider_availability_read_model";

    private readonly BusinessE2EFixture _fix;
    public ProviderAvailabilityE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var providerAvailabilityId = _fix.SeedId("provider-availability:happy:id");
        var providerId             = _fix.SeedId("provider-availability:happy:provider");
        var corrCreate             = _fix.SeedId("provider-availability:happy:corr:create");
        var corrActivate           = _fix.SeedId("provider-availability:happy:corr:activate");

        var startsAt = _fix.Clock.UtcNow;
        var endsAt   = (DateTimeOffset?)(startsAt + TimeSpan.FromDays(30));

        // 1) Create — window carries explicit StartsAt plus nullable EndsAt.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-scope/provider-availability/create",
            new CreateProviderAvailabilityRequestModel(
                providerAvailabilityId,
                providerId,
                startsAt,
                endsAt),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, providerAvailabilityId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-scope/provider-availability/activate",
            new ProviderAvailabilityIdRequestModel(providerAvailabilityId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ProviderAvailabilityReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/provider-scope/provider-availability/{providerAvailabilityId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProviderAvailabilityReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(providerAvailabilityId, read!.ProviderAvailabilityId);
        Assert.Equal("Active", read.Status);
    }
}
