using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Business.Entitlement.UsageControl.UsageRight;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Entitlement.UsageControl.UsageRight;

/// <summary>
/// E2E smoke test for the business/entitlement/usage-control/usage-right
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_entitlement_usage_right.usage_right_read_model</c>
/// table provisioned in Postgres.
///
/// Note: the domain slug <c>usage-right</c> is hyphenated in routes/topics but
/// collapses to <c>usage_right</c> in schema/table names and <c>UsageRight</c>
/// in class/namespace names.
///
/// Status lifecycle (from UsageRightAggregate.Apply + UsageRightStatus enum):
/// Available -> InUse -> Consumed.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class UsageRightE2ETests
{
    private const string ProjSchema = "projection_business_entitlement_usage_right";
    private const string ProjTable  = "usage_right_read_model";

    private readonly BusinessE2EFixture _fix;
    public UsageRightE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateUse_UpdatesProjection_GetRecordsUsage()
    {
        var usageRightId = _fix.SeedId("usage-right:happy:id");
        var subjectId    = _fix.SeedId("usage-right:happy:subject");
        var referenceId  = _fix.SeedId("usage-right:happy:reference");
        var recordId     = _fix.SeedId("usage-right:happy:record");
        var corrCreate   = _fix.SeedId("usage-right:happy:corr:create");
        var corrUse      = _fix.SeedId("usage-right:happy:corr:use");

        // 1) Create — TotalUnits must be > 0 per aggregate guard.
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/usage-right/create",
            new CreateUsageRightRequestModel(usageRightId, subjectId, referenceId, TotalUnits: 10),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, usageRightId, BusinessE2EConfig.PollTimeout);

        // 2) Use — carries a UsageRecord (RecordId + UnitsUsed). UnitsUsed
        //    must not exceed Remaining (= TotalUnits initially).
        var useResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/usage-control/usage-right/use",
            new UseUsageRightRequestModel(usageRightId, recordId, UnitsUsed: 3),
            corrUse);
        Assert.Equal(HttpStatusCode.OK, useResp.StatusCode);
        var useAck = await ApiEnvelope.ReadAsync<CommandAck>(useResp);
        Assert.NotNull(useAck);
        Assert.True(useAck!.Success);

        // 3) GET — poll until projection reflects the usage: TotalUsed > 0
        //    (also serves as proxy for Status flipping from Available to InUse).
        UsageRightReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/usage-control/usage-right/{usageRightId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<UsageRightReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.TotalUsed > 0) break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(usageRightId, read!.UsageRightId);
        Assert.Equal(subjectId, read.SubjectId);
        Assert.Equal(referenceId, read.ReferenceId);
        Assert.Equal(10, read.TotalUnits);
        Assert.True(read.TotalUsed > 0, $"Expected TotalUsed > 0 after Use; got {read.TotalUsed}.");
        Assert.Equal(recordId, read.LastRecordId);
        Assert.Equal("InUse", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
