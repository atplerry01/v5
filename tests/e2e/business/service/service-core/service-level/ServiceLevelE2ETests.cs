using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceLevel;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceCore.ServiceLevel;

/// <summary>
/// E2E smoke test for the business/service/service-core/service-level vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_service_level.service_level_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ServiceLevelE2ETests
{
    private const string ProjSchema = "projection_business_service_service_level";
    private const string ProjTable  = "service_level_read_model";

    private readonly BusinessE2EFixture _fix;
    public ServiceLevelE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var serviceLevelId      = _fix.SeedId("service-level:happy:id");
        var serviceDefinitionId = _fix.SeedId("service-level:happy:service-definition");
        var corrCreate          = _fix.SeedId("service-level:happy:corr:create");
        var corrActivate        = _fix.SeedId("service-level:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-level/create",
            new CreateServiceLevelRequestModel(
                serviceLevelId,
                serviceDefinitionId,
                "GOLD",
                "Gold Tier",
                "uptime>=99.9%"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, serviceLevelId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-level/activate",
            new ServiceLevelIdRequestModel(serviceLevelId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ServiceLevelReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-core/service-level/{serviceLevelId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ServiceLevelReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(serviceLevelId, read!.ServiceLevelId);
        Assert.Equal(serviceDefinitionId, read.ServiceDefinitionId);
        Assert.Equal("Active", read.Status);
    }
}
