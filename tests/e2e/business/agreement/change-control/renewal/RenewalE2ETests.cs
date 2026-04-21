using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.ChangeControl.Renewal;

/// <summary>
/// E2E smoke test for the business/agreement/change-control/renewal vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_renewal.renewal_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class RenewalE2ETests
{
    private const string ProjSchema = "projection_business_agreement_renewal";
    private const string ProjTable  = "renewal_read_model";

    private readonly BusinessE2EFixture _fix;
    public RenewalE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateRenew_UpdatesProjection_GetReturnsRenewed()
    {
        var renewalId  = _fix.SeedId("renewal:happy:id");
        var sourceId   = _fix.SeedId("renewal:happy:source");
        var corrCreate = _fix.SeedId("renewal:happy:corr:create");
        var corrRenew  = _fix.SeedId("renewal:happy:corr:renew");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/renewal/create",
            new CreateRenewalRequestModel(renewalId, sourceId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, renewalId, BusinessE2EConfig.PollTimeout);

        // 2) Renew
        var renewResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/change-control/renewal/renew",
            new RenewalIdRequestModel(renewalId),
            corrRenew);
        Assert.Equal(HttpStatusCode.OK, renewResp.StatusCode);
        var renewAck = await ApiEnvelope.ReadAsync<CommandAck>(renewResp);
        Assert.NotNull(renewAck);
        Assert.True(renewAck!.Success);

        // 3) GET — poll until Status flips to Renewed.
        RenewalReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/change-control/renewal/{renewalId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<RenewalReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Renewed") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(renewalId, read!.RenewalId);
        Assert.Equal(sourceId, read.SourceId);
        Assert.Equal("Renewed", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
