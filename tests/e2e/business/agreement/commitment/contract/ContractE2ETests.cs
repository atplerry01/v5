using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.Commitment.Contract;

/// <summary>
/// E2E smoke test for the business/agreement/commitment/contract Ex pilot
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded
/// ID generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_contract.contract_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ContractE2ETests
{
    private const string ProjSchema = "projection_business_agreement_contract";
    private const string ProjTable  = "contract_read_model";

    private readonly BusinessE2EFixture _fix;
    public ContractE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAddPartyActivate_UpdatesProjection_GetReturnsActive()
    {
        var contractId = _fix.SeedId("contract:happy:id");
        var partyId    = _fix.SeedId("contract:happy:party");
        var corrCreate = _fix.SeedId("contract:happy:corr:create");
        var corrParty  = _fix.SeedId("contract:happy:corr:party");
        var corrActiv  = _fix.SeedId("contract:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/contract/create",
            new CreateContractRequestModel(contractId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, contractId, BusinessE2EConfig.PollTimeout);

        // 2) AddParty — required to satisfy Activate invariant (PartyRequired).
        var partyResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/contract/add-party",
            new AddPartyRequestModel(contractId, partyId, "owner"),
            corrParty);
        Assert.Equal(HttpStatusCode.OK, partyResp.StatusCode);
        var partyAck = await ApiEnvelope.ReadAsync<CommandAck>(partyResp);
        Assert.NotNull(partyAck);
        Assert.True(partyAck!.Success);

        // 3) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/contract/activate",
            new ContractIdRequestModel(contractId),
            corrActiv);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 4) GET projection — assert Status == Active and CreatedAt populated.
        //
        // Small retry: Activate → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        ContractReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/contract/{contractId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ContractReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Active") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(contractId, read!.ContractId);
        Assert.Equal("Active", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
        Assert.Contains(read.Parties, p => p.PartyId == partyId && p.Role == "owner");
    }
}
