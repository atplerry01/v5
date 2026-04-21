using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderChange.Cancellation;

/// <summary>
/// E2E smoke test for the business/order/order-change/cancellation vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_cancellation.cancellation_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class CancellationE2ETests
{
    private const string ProjSchema = "projection_business_order_cancellation";
    private const string ProjTable  = "cancellation_read_model";

    private readonly BusinessE2EFixture _fix;
    public CancellationE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_RequestConfirm_UpdatesProjection_GetReturnsConfirmed()
    {
        var cancellationId = _fix.SeedId("order-cancellation:happy:id");
        var orderId        = _fix.SeedId("order-cancellation:happy:order");
        var corrRequest    = _fix.SeedId("order-cancellation:happy:corr:request");
        var corrConfirm    = _fix.SeedId("order-cancellation:happy:corr:confirm");

        // 1) Request
        var requestResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/cancellation/request",
            new RequestCancellationRequestModel(cancellationId, orderId, "customer request"),
            corrRequest);
        Assert.Equal(HttpStatusCode.OK, requestResp.StatusCode);
        var requestAck = await ApiEnvelope.ReadAsync<CommandAck>(requestResp);
        Assert.NotNull(requestAck);
        Assert.True(requestAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, cancellationId, BusinessE2EConfig.PollTimeout);

        // 2) Confirm
        var confirmResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/cancellation/confirm",
            new CancellationIdRequestModel(cancellationId),
            corrConfirm);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmAck = await ApiEnvelope.ReadAsync<CommandAck>(confirmResp);
        Assert.NotNull(confirmAck);
        Assert.True(confirmAck!.Success);

        // 3) GET projection — poll until Status == "Confirmed".
        CancellationReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-change/cancellation/{cancellationId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<CancellationReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Confirmed") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(cancellationId, read!.CancellationId);
        Assert.Equal(orderId, read.OrderId);
        Assert.Equal("Confirmed", read.Status);
        Assert.NotEqual(default, read.RequestedAt);
    }
}
