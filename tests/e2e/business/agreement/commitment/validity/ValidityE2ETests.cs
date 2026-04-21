using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Validity;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.Commitment.Validity;

/// <summary>
/// E2E smoke test for the business/agreement/commitment/validity vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_validity.validity_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ValidityE2ETests
{
    private const string ProjSchema = "projection_business_agreement_validity";
    private const string ProjTable  = "validity_read_model";

    private readonly BusinessE2EFixture _fix;
    public ValidityE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateExpire_UpdatesProjection_GetReturnsExpired()
    {
        var validityId = _fix.SeedId("validity:happy:id");
        var corrCreate = _fix.SeedId("validity:happy:corr:create");
        var corrExpire = _fix.SeedId("validity:happy:corr:expire");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/validity/create",
            new CreateValidityRequestModel(validityId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, validityId, BusinessE2EConfig.PollTimeout);

        // 2) Expire
        var expireResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/commitment/validity/expire",
            new ValidityIdRequestModel(validityId),
            corrExpire);
        Assert.Equal(HttpStatusCode.OK, expireResp.StatusCode);
        var expireAck = await ApiEnvelope.ReadAsync<CommandAck>(expireResp);
        Assert.NotNull(expireAck);
        Assert.True(expireAck!.Success);

        // 3) GET projection — assert Status == Expired and CreatedAt populated.
        //
        // Small retry: Expire → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        ValidityReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/commitment/validity/{validityId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ValidityReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Expired") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(validityId, read!.ValidityId);
        Assert.Equal("Expired", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
