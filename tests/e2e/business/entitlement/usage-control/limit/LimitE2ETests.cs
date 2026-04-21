using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.Limit;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Entitlement.UsageControl.Limit;

/// <summary>
/// E2E smoke test for the business/entitlement/usage-control/limit vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_limit.limit_read_model</c> table
/// provisioned in Postgres.
///
/// Status lifecycle (from LimitAggregate.Apply + LimitStatus enum):
/// Defined -> Enforced -> Breached.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class LimitE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_limit";
    private const string ProjTable  = "limit_read_model";

    private readonly BusinessE2EFixture _fix;
    public LimitE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateEnforce_UpdatesProjection_GetReturnsEnforced()
    {
        var limitId     = _fix.SeedId("limit:happy:id");
        var subjectId   = _fix.SeedId("limit:happy:subject");
        var corrCreate  = _fix.SeedId("limit:happy:corr:create");
        var corrEnforce = _fix.SeedId("limit:happy:corr:enforce");

        // 1) Create — ThresholdValue must be > 0 per aggregate guard.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/limit/create",
            new CreateLimitRequestModel(limitId, subjectId, ThresholdValue: 100),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, limitId, BusinessE2EConfig.PollTimeout);

        // 2) Enforce
        var enforceResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/limit/enforce",
            new LimitIdRequestModel(limitId),
            corrEnforce);
        Assert.Equal(HttpStatusCode.OK, enforceResp.StatusCode);
        var enforceAck = await ApiEnvelope.ReadAsync<CommandAck>(enforceResp);
        Assert.NotNull(enforceAck);
        Assert.True(enforceAck!.Success);

        // 3) GET — poll until Status flips to Enforced.
        LimitReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/usage-control/limit/{limitId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<LimitReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Enforced") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(limitId, read!.LimitId);
        Assert.Equal(subjectId, read.SubjectId);
        Assert.Equal(100, read.ThresholdValue);
        Assert.Equal("Enforced", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
