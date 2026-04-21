using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Business.Provider.ProviderGovernance.ProviderAgreement;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Provider.ProviderGovernance.ProviderAgreement;

/// <summary>
/// E2E smoke test for the business/provider/provider-governance/provider-agreement
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_provider_provider_agreement.provider_agreement_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProviderAgreementE2ETests
{
    private const string ProjSchema = "projection_business_provider_provider_agreement";
    private const string ProjTable  = "provider_agreement_read_model";

    private readonly BusinessE2EFixture _fix;
    public ProviderAgreementE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateActivate_UpdatesProjection_GetReturnsActive()
    {
        var providerAgreementId = _fix.SeedId("provider-agreement:happy:id");
        var providerId          = _fix.SeedId("provider-agreement:happy:provider");
        var corrCreate          = _fix.SeedId("provider-agreement:happy:corr:create");
        var corrActivate        = _fix.SeedId("provider-agreement:happy:corr:activate");

        // Activation time-window is supplied explicitly so the dispatch seed
        // derives only from stable command coordinates (per DET-SEED-DERIVATION-01).
        var startsAt = _fix.Clock.UtcNow;
        var endsAt   = startsAt + TimeSpan.FromDays(365);

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-governance/provider-agreement/create",
            new CreateProviderAgreementRequestModel(
                providerAgreementId,
                providerId,
                ContractId: null,
                EffectiveStartsAt: null,
                EffectiveEndsAt: null),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, providerAgreementId, BusinessE2EConfig.PollTimeout);

        // 2) Activate — passes explicit StartsAt/EndsAt to satisfy the
        // ProviderAgreement invariant that Active state has an effective window.
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-governance/provider-agreement/activate",
            new ActivateProviderAgreementRequestModel(
                providerAgreementId,
                startsAt,
                endsAt),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 3) GET projection — poll until Status == "Active".
        ProviderAgreementReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/provider-governance/provider-agreement/{providerAgreementId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProviderAgreementReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(providerAgreementId, read!.ProviderAgreementId);
        Assert.Equal("Active", read.Status);
    }
}
