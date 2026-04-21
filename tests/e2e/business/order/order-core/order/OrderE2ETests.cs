using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.Order;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderCore.Order;

/// <summary>
/// E2E smoke test for the business/order/order-core/order vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_order.order_read_model</c> table provisioned
/// in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class OrderE2ETests
{
    private const string ProjSchema = "projection_business_order_order";
    private const string ProjTable  = "order_read_model";

    private readonly BusinessE2EFixture _fix;
    public OrderE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateConfirm_UpdatesProjection_GetReturnsConfirmed()
    {
        var orderId          = _fix.SeedId("order:happy:id");
        var sourceReferenceId = _fix.SeedId("order:happy:source");
        var corrCreate       = _fix.SeedId("order:happy:corr:create");
        var corrConfirm      = _fix.SeedId("order:happy:corr:confirm");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/order/create",
            new CreateOrderRequestModel(orderId, sourceReferenceId, "E2E smoke order"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, orderId, BusinessE2EConfig.PollTimeout);

        // 2) Confirm
        var confirmResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/order/confirm",
            new OrderIdRequestModel(orderId),
            corrConfirm);
        Assert.Equal(HttpStatusCode.OK, confirmResp.StatusCode);
        var confirmAck = await ApiEnvelope.ReadAsync<CommandAck>(confirmResp);
        Assert.NotNull(confirmAck);
        Assert.True(confirmAck!.Success);

        // 3) GET projection — poll until Status == "Confirmed".
        OrderReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-core/order/{orderId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<OrderReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Confirmed") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(orderId, read!.OrderId);
        Assert.Equal(sourceReferenceId, read.SourceReferenceId);
        Assert.Equal("Confirmed", read.Status);
    }
}
