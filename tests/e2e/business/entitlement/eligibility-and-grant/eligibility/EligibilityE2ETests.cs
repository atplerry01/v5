using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Eligibility;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Entitlement.EligibilityAndGrant.Eligibility;

/// <summary>
/// E2E smoke test for the business/entitlement/eligibility-and-grant/eligibility
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_eligibility.eligibility_read_model</c>
/// table provisioned in Postgres.
///
/// Status lifecycle (from EligibilityAggregate.Apply + EligibilityStatus enum):
/// Pending -> Eligible | Ineligible.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class EligibilityE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_eligibility";
    private const string ProjTable  = "eligibility_read_model";

    private readonly BusinessE2EFixture _fix;
    public EligibilityE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateMarkEligible_UpdatesProjection_GetReturnsEligible()
    {
        var eligibilityId = _fix.SeedId("eligibility:happy:id");
        var subjectId     = _fix.SeedId("eligibility:happy:subject");
        var targetId      = _fix.SeedId("eligibility:happy:target");
        var corrCreate    = _fix.SeedId("eligibility:happy:corr:create");
        var corrEligible  = _fix.SeedId("eligibility:happy:corr:eligible");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/eligibility/create",
            new CreateEligibilityRequestModel(eligibilityId, subjectId, targetId, "default"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, eligibilityId, BusinessE2EConfig.PollTimeout);

        // 2) MarkEligible
        var eligibleResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/eligibility/mark-eligible",
            new EligibilityIdRequestModel(eligibilityId),
            corrEligible);
        Assert.Equal(HttpStatusCode.OK, eligibleResp.StatusCode);
        var eligibleAck = await ApiEnvelope.ReadAsync<CommandAck>(eligibleResp);
        Assert.NotNull(eligibleAck);
        Assert.True(eligibleAck!.Success);

        // 3) GET — poll until Status flips to Eligible.
        EligibilityReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/eligibility-and-grant/eligibility/{eligibilityId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<EligibilityReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Eligible") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(eligibilityId, read!.EligibilityId);
        Assert.Equal(subjectId, read.SubjectId);
        Assert.Equal(targetId, read.TargetId);
        Assert.Equal("Eligible", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
