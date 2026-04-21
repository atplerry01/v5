using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Amendment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.ChangeControl.Amendment;

/// <summary>
/// E2E smoke test for the business/agreement/change-control/amendment vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_amendment.amendment_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class AmendmentE2ETests
{
    private const string ProjSchema = "projection_business_agreement_amendment";
    private const string ProjTable  = "amendment_read_model";

    private readonly BusinessE2EFixture _fix;
    public AmendmentE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateApply_UpdatesProjection_GetReturnsApplied()
    {
        var amendmentId = _fix.SeedId("amendment:happy:id");
        var targetId    = _fix.SeedId("amendment:happy:target");
        var corrCreate  = _fix.SeedId("amendment:happy:corr:create");
        var corrApply   = _fix.SeedId("amendment:happy:corr:apply");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/amendment/create",
            new CreateAmendmentRequestModel(amendmentId, targetId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, amendmentId, BusinessE2EConfig.PollTimeout);

        // 2) Apply
        var applyResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/amendment/apply",
            new AmendmentIdRequestModel(amendmentId),
            corrApply);
        Assert.Equal(HttpStatusCode.OK, applyResp.StatusCode);
        var applyAck = await ApiEnvelope.ReadAsync<CommandAck>(applyResp);
        Assert.NotNull(applyAck);
        Assert.True(applyAck!.Success);

        // 3) GET — poll until Status flips to Applied.
        AmendmentReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/change-control/amendment/{amendmentId}");
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
        Assert.Equal(targetId, read.TargetId);
        Assert.Equal("Applied", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
