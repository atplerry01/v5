using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Approval;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.ChangeControl.Approval;

/// <summary>
/// E2E smoke test for the business/agreement/change-control/approval vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_approval.approval_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ApprovalE2ETests
{
    private const string ProjSchema = "projection_business_agreement_approval";
    private const string ProjTable  = "approval_read_model";

    private readonly BusinessE2EFixture _fix;
    public ApprovalE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateApprove_UpdatesProjection_GetReturnsApproved()
    {
        var approvalId  = _fix.SeedId("approval:happy:id");
        var corrCreate  = _fix.SeedId("approval:happy:corr:create");
        var corrApprove = _fix.SeedId("approval:happy:corr:approve");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/approval/create",
            new CreateApprovalRequestModel(approvalId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, approvalId, BusinessE2EConfig.PollTimeout);

        // 2) Approve
        var approveResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/approval/approve",
            new ApprovalIdRequestModel(approvalId),
            corrApprove);
        Assert.Equal(HttpStatusCode.OK, approveResp.StatusCode);
        var approveAck = await ApiEnvelope.ReadAsync<CommandAck>(approveResp);
        Assert.NotNull(approveAck);
        Assert.True(approveAck!.Success);

        // 3) GET — poll until Status flips to Approved.
        ApprovalReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/change-control/approval/{approvalId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ApprovalReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Approved") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(approvalId, read!.ApprovalId);
        Assert.Equal("Approved", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
