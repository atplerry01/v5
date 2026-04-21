using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Business.Order.OrderCore.LineItem;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderCore.LineItem;

/// <summary>
/// E2E smoke test for the business/order/order-core/line-item vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_line_item.line_item_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class LineItemE2ETests
{
    private const string ProjSchema = "projection_business_order_line_item";
    private const string ProjTable  = "line_item_read_model";

    private readonly BusinessE2EFixture _fix;
    public LineItemE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateUpdateQuantity_UpdatesProjection_GetReturnsUpdatedQuantity()
    {
        var lineItemId  = _fix.SeedId("line-item:happy:id");
        var orderId     = _fix.SeedId("line-item:happy:order");
        var subjectId   = _fix.SeedId("line-item:happy:subject");
        var corrCreate  = _fix.SeedId("line-item:happy:corr:create");
        var corrUpdate  = _fix.SeedId("line-item:happy:corr:update");

        const int subjectKind = 1;
        const decimal initialQuantity = 3m;
        const decimal updatedQuantity = 7m;
        const string unit = "unit";

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/line-item/create",
            new CreateLineItemRequestModel(
                lineItemId,
                orderId,
                subjectKind,
                subjectId,
                initialQuantity,
                unit),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, lineItemId, BusinessE2EConfig.PollTimeout);

        // 2) UpdateQuantity
        var updateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-core/line-item/update-quantity",
            new UpdateLineItemQuantityRequestModel(lineItemId, updatedQuantity, unit),
            corrUpdate);
        Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);
        var updateAck = await ApiEnvelope.ReadAsync<CommandAck>(updateResp);
        Assert.NotNull(updateAck);
        Assert.True(updateAck!.Success);

        // 3) GET projection — poll until QuantityValue flips to updatedQuantity.
        // LineItem's update-quantity reducer preserves Status ("Requested") and only
        // changes QuantityValue/QuantityUnit, so we assert on Quantity.
        LineItemReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-core/line-item/{lineItemId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<LineItemReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.QuantityValue == updatedQuantity) break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(lineItemId, read!.LineItemId);
        Assert.Equal(orderId, read.OrderId);
        Assert.Equal(updatedQuantity, read.QuantityValue);
        Assert.Equal(unit, read.QuantityUnit);
        Assert.Equal("Requested", read.Status);
    }
}
