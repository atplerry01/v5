using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Obligation;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.Commitment.Obligation;

/// <summary>
/// E2E smoke test for the business/agreement/commitment/obligation vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_obligation.obligation_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ObligationE2ETests
{
    private const string ProjSchema = "projection_business_agreement_obligation";
    private const string ProjTable  = "obligation_read_model";

    private readonly BusinessE2EFixture _fix;
    public ObligationE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateFulfill_UpdatesProjection_GetReturnsFulfilled()
    {
        var obligationId = _fix.SeedId("obligation:happy:id");
        var corrCreate   = _fix.SeedId("obligation:happy:corr:create");
        var corrFulfill  = _fix.SeedId("obligation:happy:corr:fulfill");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/obligation/create",
            new CreateObligationRequestModel(obligationId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, obligationId, BusinessE2EConfig.PollTimeout);

        // 2) Fulfill
        var fulfillResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/obligation/fulfill",
            new ObligationIdRequestModel(obligationId),
            corrFulfill);
        Assert.Equal(HttpStatusCode.OK, fulfillResp.StatusCode);
        var fulfillAck = await ApiEnvelope.ReadAsync<CommandAck>(fulfillResp);
        Assert.NotNull(fulfillAck);
        Assert.True(fulfillAck!.Success);

        // 3) GET projection — assert Status == Fulfilled and CreatedAt populated.
        //
        // Small retry: Fulfill → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        ObligationReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/commitment/obligation/{obligationId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ObligationReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Fulfilled") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(obligationId, read!.ObligationId);
        Assert.Equal("Fulfilled", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
