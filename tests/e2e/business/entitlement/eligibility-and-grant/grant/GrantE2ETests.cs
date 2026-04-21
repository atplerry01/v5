using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Entitlement.EligibilityAndGrant.Grant;

/// <summary>
/// E2E smoke test for the business/entitlement/eligibility-and-grant/grant
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_grant.grant_read_model</c> table
/// provisioned in Postgres.
///
/// Status lifecycle (from GrantAggregate.Apply + GrantStatus enum):
/// Pending -> Active -> Revoked | Expired.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class GrantE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_grant";
    private const string ProjTable  = "grant_read_model";

    private readonly BusinessE2EFixture _fix;
    public GrantE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var grantId      = _fix.SeedId("grant:happy:id");
        var subjectId    = _fix.SeedId("grant:happy:subject");
        var targetId     = _fix.SeedId("grant:happy:target");
        var corrCreate   = _fix.SeedId("grant:happy:corr:create");
        var corrActivate = _fix.SeedId("grant:happy:corr:activate");

        // 1) Create — leave ExpiresAt null so Activate does not trip the
        //   expiry-in-past guard in GrantAggregate.Activate.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/grant/create",
            new CreateGrantRequestModel(grantId, subjectId, targetId, "default", ExpiresAt: null),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, grantId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/grant/activate",
            new GrantIdRequestModel(grantId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        GrantReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/eligibility-and-grant/grant/{grantId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<GrantReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(grantId, read!.GrantId);
        Assert.Equal(subjectId, read.SubjectId);
        Assert.Equal(targetId, read.TargetId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
