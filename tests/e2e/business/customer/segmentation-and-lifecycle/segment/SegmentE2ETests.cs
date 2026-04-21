using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Segment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Customer.SegmentationAndLifecycle.Segment;

/// <summary>
/// E2E smoke test for the business/customer/segmentation-and-lifecycle/segment
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_customer_segment.segment_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class SegmentE2ETests
{
    private const string ProjSchema = "projection_business_customer_segment";
    private const string ProjTable  = "segment_read_model";

    private readonly BusinessE2EFixture _fix;
    public SegmentE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var segmentId  = _fix.SeedId("segment:happy:id");
        var corrCreate = _fix.SeedId("segment:happy:corr:create");
        var corrActiv  = _fix.SeedId("segment:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/segment/create",
            new CreateSegmentRequestModel(segmentId, "VIP-001", "VIP Customers", "Behavioral", "tier=='vip'"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, segmentId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/segmentation-and-lifecycle/segment/activate",
            new SegmentIdRequestModel(segmentId),
            corrActiv);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        SegmentReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/segmentation-and-lifecycle/segment/{segmentId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<SegmentReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(segmentId, read!.SegmentId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.LastUpdatedAt);
    }
}
