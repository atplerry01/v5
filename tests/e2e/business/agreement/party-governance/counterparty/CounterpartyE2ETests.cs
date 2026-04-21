using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.PartyGovernance.Counterparty;

/// <summary>
/// E2E smoke test for the business/agreement/party-governance/counterparty vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_counterparty.counterparty_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class CounterpartyE2ETests
{
    private const string ProjSchema = "projection_business_agreement_counterparty";
    private const string ProjTable  = "counterparty_read_model";

    private readonly BusinessE2EFixture _fix;
    public CounterpartyE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateSuspend_UpdatesProjection_GetReturnsSuspended()
    {
        var counterpartyId    = _fix.SeedId("counterparty:happy:id");
        var identityReference = _fix.SeedId("counterparty:happy:identity");
        var corrCreate        = _fix.SeedId("counterparty:happy:corr:create");
        var corrSuspend       = _fix.SeedId("counterparty:happy:corr:suspend");

        // 1) Create — counterparty lands in Active state on creation (no separate Activate command).
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/party-governance/counterparty/create",
            new CreateCounterpartyRequestModel(counterpartyId, identityReference, "ACME Corp"),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, counterpartyId, BusinessE2EConfig.PollTimeout);

        // 2) Suspend
        var suspendResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/party-governance/counterparty/suspend",
            new CounterpartyIdRequestModel(counterpartyId),
            corrSuspend);
        Assert.Equal(HttpStatusCode.OK, suspendResp.StatusCode);
        var suspendAck = await ApiEnvelope.ReadAsync<CommandAck>(suspendResp);
        Assert.NotNull(suspendAck);
        Assert.True(suspendAck!.Success);

        // 3) GET projection — assert Status == Suspended and CreatedAt populated.
        //
        // Small retry: Suspend → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        CounterpartyReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/party-governance/counterparty/{counterpartyId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<CounterpartyReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Suspended") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(counterpartyId, read!.CounterpartyId);
        Assert.Equal("Suspended", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
