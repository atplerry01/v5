using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Business.Service.ServiceCore.ServiceDefinition;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceCore.ServiceDefinition;

/// <summary>
/// E2E smoke test for the business/service/service-core/service-definition vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_service_definition.service_definition_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ServiceDefinitionE2ETests
{
    private const string ProjSchema = "projection_business_service_service_definition";
    private const string ProjTable  = "service_definition_read_model";

    private readonly BusinessE2EFixture _fix;
    public ServiceDefinitionE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var serviceDefinitionId = _fix.SeedId("service-definition:happy:id");
        var corrCreate          = _fix.SeedId("service-definition:happy:corr:create");
        var corrActivate        = _fix.SeedId("service-definition:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-definition/create",
            new CreateServiceDefinitionRequestModel(
                serviceDefinitionId,
                "Managed Hosting",
                "infrastructure"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, serviceDefinitionId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-core/service-definition/activate",
            new ServiceDefinitionIdRequestModel(serviceDefinitionId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ServiceDefinitionReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-core/service-definition/{serviceDefinitionId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ServiceDefinitionReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(serviceDefinitionId, read!.ServiceDefinitionId);
        Assert.Equal("Active", read.Status);
    }
}
