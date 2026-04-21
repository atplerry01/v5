using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Business.Service.ServiceConstraint.ServiceConstraint;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Service.ServiceConstraint.ServiceConstraint;

/// <summary>
/// E2E smoke test for the business/service/service-constraint/service-constraint vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_service_service_constraint.service_constraint_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ServiceConstraintE2ETests
{
    private const string ProjSchema = "projection_business_service_service_constraint";
    private const string ProjTable  = "service_constraint_read_model";

    private readonly BusinessE2EFixture _fix;
    public ServiceConstraintE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var serviceConstraintId = _fix.SeedId("service-constraint:happy:id");
        var serviceDefinitionId = _fix.SeedId("service-constraint:happy:service-definition");
        var corrCreate          = _fix.SeedId("service-constraint:happy:corr:create");
        var corrActivate        = _fix.SeedId("service-constraint:happy:corr:activate");

        // 1) Create — Kind=0 corresponds to ConstraintKind.Availability.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/service-constraint/create",
            new CreateServiceConstraintRequestModel(
                serviceConstraintId,
                serviceDefinitionId,
                0,
                "availability: 24x7"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, serviceConstraintId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/service-constraint/service-constraint/activate",
            new ServiceConstraintIdRequestModel(serviceConstraintId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ServiceConstraintReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/service-constraint/service-constraint/{serviceConstraintId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ServiceConstraintReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(serviceConstraintId, read!.ServiceConstraintId);
        Assert.Equal(serviceDefinitionId, read.ServiceDefinitionId);
        Assert.Equal("Active", read.Status);
    }
}
