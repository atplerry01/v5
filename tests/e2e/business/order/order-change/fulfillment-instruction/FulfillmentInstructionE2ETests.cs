using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.FulfillmentInstruction;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Order.OrderChange.FulfillmentInstruction;

/// <summary>
/// E2E smoke test for the business/order/order-change/fulfillment-instruction vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_order_fulfillment_instruction.fulfillment_instruction_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class FulfillmentInstructionE2ETests
{
    private const string ProjSchema = "projection_business_order_fulfillment_instruction";
    private const string ProjTable  = "fulfillment_instruction_read_model";

    private readonly BusinessE2EFixture _fix;
    public FulfillmentInstructionE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateIssue_UpdatesProjection_GetReturnsIssued()
    {
        var fulfillmentInstructionId = _fix.SeedId("fulfillment-instruction:happy:id");
        var orderId                  = _fix.SeedId("fulfillment-instruction:happy:order");
        var lineItemId               = _fix.SeedId("fulfillment-instruction:happy:lineitem");
        var corrCreate               = _fix.SeedId("fulfillment-instruction:happy:corr:create");
        var corrIssue                = _fix.SeedId("fulfillment-instruction:happy:corr:issue");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/fulfillment-instruction/create",
            new CreateFulfillmentInstructionRequestModel(
                fulfillmentInstructionId,
                orderId,
                "ship north-warehouse",
                lineItemId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, fulfillmentInstructionId, BusinessE2EConfig.PollTimeout);

        // 2) Issue
        var issueResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/order-change/fulfillment-instruction/issue",
            new FulfillmentInstructionIdRequestModel(fulfillmentInstructionId),
            corrIssue);
        Assert.Equal(HttpStatusCode.OK, issueResp.StatusCode);
        var issueAck = await ApiEnvelope.ReadAsync<CommandAck>(issueResp);
        Assert.NotNull(issueAck);
        Assert.True(issueAck!.Success);

        // 3) GET projection — poll until Status == "Issued".
        FulfillmentInstructionReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/order-change/fulfillment-instruction/{fulfillmentInstructionId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<FulfillmentInstructionReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Issued") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(fulfillmentInstructionId, read!.FulfillmentInstructionId);
        Assert.Equal(orderId, read.OrderId);
        Assert.Equal("Issued", read.Status);
    }
}
