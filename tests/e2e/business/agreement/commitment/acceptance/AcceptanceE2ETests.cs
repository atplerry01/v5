using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Acceptance;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.Commitment.Acceptance;

/// <summary>
/// E2E smoke test for the business/agreement/commitment/acceptance vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_acceptance.acceptance_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class AcceptanceE2ETests
{
    private const string ProjSchema = "projection_business_agreement_acceptance";
    private const string ProjTable  = "acceptance_read_model";

    private readonly BusinessE2EFixture _fix;
    public AcceptanceE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAccept_UpdatesProjection_GetReturnsAccepted()
    {
        var acceptanceId = _fix.SeedId("acceptance:happy:id");
        var corrCreate   = _fix.SeedId("acceptance:happy:corr:create");
        var corrAccept   = _fix.SeedId("acceptance:happy:corr:accept");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/acceptance/create",
            new CreateAcceptanceRequestModel(acceptanceId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, acceptanceId, BusinessE2EConfig.PollTimeout);

        // 2) Accept
        var acceptResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/acceptance/accept",
            new AcceptanceIdRequestModel(acceptanceId),
            corrAccept);
        Assert.Equal(HttpStatusCode.OK, acceptResp.StatusCode);
        var acceptAck = await ApiEnvelope.ReadAsync<CommandAck>(acceptResp);
        Assert.NotNull(acceptAck);
        Assert.True(acceptAck!.Success);

        // 3) GET projection — assert Status == Accepted and CreatedAt populated.
        //
        // Small retry: Accept → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        AcceptanceReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/commitment/acceptance/{acceptanceId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<AcceptanceReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Accepted") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(acceptanceId, read!.AcceptanceId);
        Assert.Equal("Accepted", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
