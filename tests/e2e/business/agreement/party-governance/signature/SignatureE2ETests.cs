using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Agreement.PartyGovernance.Signature;

/// <summary>
/// E2E smoke test for the business/agreement/party-governance/signature vertical.
/// Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID generation,
/// wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_agreement_signature.signature_read_model</c> table
/// provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class SignatureE2ETests
{
    private const string ProjSchema = "projection_business_agreement_signature";
    private const string ProjTable  = "signature_read_model";

    private readonly BusinessE2EFixture _fix;
    public SignatureE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateSign_UpdatesProjection_GetReturnsSigned()
    {
        var signatureId = _fix.SeedId("signature:happy:id");
        var corrCreate  = _fix.SeedId("signature:happy:corr:create");
        var corrSign    = _fix.SeedId("signature:happy:corr:sign");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/party-governance/signature/create",
            new SignatureIdRequestModel(signatureId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, signatureId, BusinessE2EConfig.PollTimeout);

        // 2) Sign
        var signResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/party-governance/signature/sign",
            new SignatureIdRequestModel(signatureId),
            corrSign);
        Assert.Equal(HttpStatusCode.OK, signResp.StatusCode);
        var signAck = await ApiEnvelope.ReadAsync<CommandAck>(signResp);
        Assert.NotNull(signAck);
        Assert.True(signAck!.Success);

        // 3) GET projection — assert Status == Signed and CreatedAt populated.
        //
        // Small retry: Sign → projection-row update is async (Kafka hop).
        // We already know the row exists from step 1 so we poll until Status flips.
        SignatureReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/party-governance/signature/{signatureId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<SignatureReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null && read.Status == "Signed") break;
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(signatureId, read!.SignatureId);
        Assert.Equal("Signed", read.Status);
        Assert.NotEqual(default, read.CreatedAt);
    }
}
