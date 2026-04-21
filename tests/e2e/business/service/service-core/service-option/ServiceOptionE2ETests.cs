using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceOption;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceCore.ServiceOption;

/// <summary>
/// E2E smoke test for the business/service/service-core/service-option vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_service_option.service_option_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ServiceOptionE2ETests
{
    private const string ProjSchema = "projection_business_service_service_option";
    private const string ProjTable  = "service_option_read_model";

    private readonly BusinessE2EFixture _fix;
    public ServiceOptionE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var serviceOptionId     = _fix.SeedId("service-option:happy:id");
        var serviceDefinitionId = _fix.SeedId("service-option:happy:service-definition");
        var corrCreate          = _fix.SeedId("service-option:happy:corr:create");
        var corrActivate        = _fix.SeedId("service-option:happy:corr:activate");

        // 1) Create — Kind must parse to a valid OptionKind (Feature/Capacity/Variant).
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-option/create",
            new CreateServiceOptionRequestModel(
                serviceOptionId,
                serviceDefinitionId,
                "OPT-A",
                "Primary Option",
                "Feature"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, serviceOptionId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-option/activate",
            new ServiceOptionIdRequestModel(serviceOptionId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ServiceOptionReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-core/service-option/{serviceOptionId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ServiceOptionReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(serviceOptionId, read!.ServiceOptionId);
        Assert.Equal(serviceDefinitionId, read.ServiceDefinitionId);
        Assert.Equal("Active", read.Status);
    }
}
