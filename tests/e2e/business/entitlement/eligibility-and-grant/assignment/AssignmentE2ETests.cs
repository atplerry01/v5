using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Assignment;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Entitlement.EligibilityAndGrant.Assignment;

/// <summary>
/// E2E smoke test for the business/entitlement/eligibility-and-grant/assignment
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_assignment.assignment_read_model</c>
/// table provisioned in Postgres.
///
/// Status lifecycle (from AssignmentAggregate.Apply + AssignmentStatus enum):
/// Pending -> Active -> Revoked.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class AssignmentE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_assignment";
    private const string ProjTable  = "assignment_read_model";

    private readonly BusinessE2EFixture _fix;
    public AssignmentE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var assignmentId = _fix.SeedId("assignment:happy:id");
        var grantId      = _fix.SeedId("assignment:happy:grant");
        var subjectId    = _fix.SeedId("assignment:happy:subject");
        var corrCreate   = _fix.SeedId("assignment:happy:corr:create");
        var corrActivate = _fix.SeedId("assignment:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/assignment/create",
            new CreateAssignmentRequestModel(assignmentId, grantId, subjectId, "default"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, assignmentId, BusinessE2EConfig.PollTimeout);

        // 2) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/eligibility-and-grant/assignment/activate",
            new AssignmentIdRequestModel(assignmentId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET — poll until Status flips to Active.
        AssignmentReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/eligibility-and-grant/assignment/{assignmentId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<AssignmentReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(assignmentId, read!.AssignmentId);
        Assert.Equal(grantId, read.GrantId);
        Assert.Equal(subjectId, read.SubjectId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
