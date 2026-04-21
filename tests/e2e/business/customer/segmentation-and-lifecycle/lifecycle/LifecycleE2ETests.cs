using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Customer.SegmentationAndLifecycle.Lifecycle;

/// <summary>
/// E2E smoke test for the business/customer/segmentation-and-lifecycle/lifecycle
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_customer_lifecycle.lifecycle_read_model</c> table
/// provisioned in Postgres.
///
/// Canonical stage progression (see CanChangeStageSpecification):
///   Prospect -> Onboarded -> Active -> Dormant|Churned, Dormant -> Active|Churned.
/// The happy path walks Prospect -> Onboarded (single legal next-stage hop).
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class LifecycleE2ETests
{
    private const string ProjSchema = "projection_business_customer_lifecycle";
    private const string ProjTable  = "lifecycle_read_model";

    private readonly BusinessE2EFixture _fix;
    public LifecycleE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_StartChangeStage_UpdatesProjection_GetReturnsNextStage()
    {
        var lifecycleId = _fix.SeedId("lifecycle:happy:id");
        var customerId  = _fix.SeedId("lifecycle:happy:customer");
        var corrStart   = _fix.SeedId("lifecycle:happy:corr:start");
        var corrChange  = _fix.SeedId("lifecycle:happy:corr:change");

        // 1) Start — initial stage is Prospect (first canonical stage).
        var startResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/lifecycle/start",
            new StartLifecycleRequestModel(lifecycleId, customerId, "Prospect"),
            corrStart);
        Assert.Equal(HttpStatusCode.OK, startResp.StatusCode);
        var startAck = await ApiEnvelope.ReadAsync<CommandAck>(startResp);
        Assert.NotNull(startAck);
        Assert.True(startAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, lifecycleId, BusinessE2EConfig.PollTimeout);

        // 2) ChangeStage — Prospect -> Onboarded (the only legal next stage).
        var changeResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/lifecycle/change-stage",
            new ChangeLifecycleStageRequestModel(lifecycleId, "Onboarded"),
            corrChange);
        Assert.Equal(HttpStatusCode.OK, changeResp.StatusCode);
        var changeAck = await ApiEnvelope.ReadAsync<CommandAck>(changeResp);
        Assert.NotNull(changeAck);
        Assert.True(changeAck!.Success);

        // 3) GET — poll until Stage advances to Onboarded.
        LifecycleReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/segmentation-and-lifecycle/lifecycle/{lifecycleId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<LifecycleReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Stage == "Onboarded") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(lifecycleId, read!.LifecycleId);
        Assert.Equal(customerId, read.CustomerId);
        Assert.Equal("Onboarded", read.Stage);
        Assert.Equal("Tracking", read.Status);
        Assert.NotEqual(default, read.StartedAt);
        Assert.NotEqual(default, read.LastUpdatedAt);
    }
}
