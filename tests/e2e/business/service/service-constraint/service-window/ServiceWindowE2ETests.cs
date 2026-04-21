using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceWindow;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceConstraint.ServiceWindow;

/// <summary>
/// E2E smoke test for the business/service/service-constraint/service-window vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_service_window.service_window_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ServiceWindowE2ETests
{
    private const string ProjSchema = "projection_business_service_service_window";
    private const string ProjTable  = "service_window_read_model";

    private readonly BusinessE2EFixture _fix;
    public ServiceWindowE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var serviceWindowId     = _fix.SeedId("service-window:happy:id");
        var serviceDefinitionId = _fix.SeedId("service-window:happy:service-definition");
        var corrCreate          = _fix.SeedId("service-window:happy:corr:create");
        var corrActivate        = _fix.SeedId("service-window:happy:corr:activate");

        // Deterministic bounds — StartsAt fixed in the past, EndsAt 30 days later.
        var startsAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var endsAt   = startsAt.AddDays(30);

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/service-window/create",
            new CreateServiceWindowRequestModel(
                serviceWindowId,
                serviceDefinitionId,
                startsAt,
                endsAt),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, serviceWindowId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/service-window/activate",
            new ServiceWindowIdRequestModel(serviceWindowId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ServiceWindowReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-constraint/service-window/{serviceWindowId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ServiceWindowReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(serviceWindowId, read!.ServiceWindowId);
        Assert.Equal(serviceDefinitionId, read.ServiceDefinitionId);
        Assert.Equal("Active", read.Status);
    }
}
