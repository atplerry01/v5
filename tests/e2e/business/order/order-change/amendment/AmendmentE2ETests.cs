using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Amendment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderChange.Amendment;

/// <summary>
/// E2E smoke test for the business/order/order-change/amendment vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_amendment.amendment_read_model</c> table
/// provisioned in Postgres.
///
/// Note: The Amendment BC name collides with agreement/change-control/amendment;
/// this file is namespaced under Order.OrderChange.Amendment and bound to the
/// order-scoped controller/contract to disambiguate at compile time.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class AmendmentE2ETests
{
    private const string ProjSchema = "projection_business_order_amendment";
    private const string ProjTable  = "amendment_read_model";

    private readonly BusinessE2EFixture _fix;
    public AmendmentE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_RequestAcceptApply_UpdatesProjection_GetReturnsApplied()
    {
        var amendmentId = _fix.SeedId("order-amendment:happy:id");
        var orderId     = _fix.SeedId("order-amendment:happy:order");
        var corrRequest = _fix.SeedId("order-amendment:happy:corr:request");
        var corrAccept  = _fix.SeedId("order-amendment:happy:corr:accept");
        var corrApply   = _fix.SeedId("order-amendment:happy:corr:apply");

        // 1) Request
        var requestResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/amendment/request",
            new RequestAmendmentRequestModel(amendmentId, orderId, "price correction"),
            corrRequest);
        Assert.Equal(HttpStatusCode.OK, requestResp.StatusCode);
        var requestAck = await ApiEnvelope.ReadAsync<CommandAck>(requestResp);
        Assert.NotNull(requestAck);
        Assert.True(requestAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, amendmentId, BusinessE2EConfig.PollTimeout);

        // 2) Accept
        var acceptResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/amendment/accept",
            new AmendmentIdRequestModel(amendmentId),
            corrAccept);
        Assert.Equal(HttpStatusCode.OK, acceptResp.StatusCode);
        var acceptAck = await ApiEnvelope.ReadAsync<CommandAck>(acceptResp);
        Assert.NotNull(acceptAck);
        Assert.True(acceptAck!.Success);

        // 3) Apply
        var applyResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/amendment/apply",
            new AmendmentIdRequestModel(amendmentId),
            corrApply);
        Assert.Equal(HttpStatusCode.OK, applyResp.StatusCode);
        var applyAck = await ApiEnvelope.ReadAsync<CommandAck>(applyResp);
        Assert.NotNull(applyAck);
        Assert.True(applyAck!.Success);

        // 4) GET projection — poll until Status == "Applied" (AmendmentStatus.Applied).
        AmendmentReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-change/amendment/{amendmentId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<AmendmentReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Applied") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(amendmentId, read!.AmendmentId);
        Assert.Equal(orderId, read.OrderId);
        Assert.Equal("Applied", read.Status);
        Assert.NotEqual(default, read.RequestedAt);
    }
}
