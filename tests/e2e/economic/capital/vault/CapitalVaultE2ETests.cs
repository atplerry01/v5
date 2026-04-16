using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;

namespace Whycespace.Tests.E2E.Economic.Capital.Vault;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalVaultE2ETests
{
    private const string ProjSchema = "projection_economic_capital_vault";
    private const string ProjTable  = "capital_vault_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalVaultE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateVault_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var ownerId = _fix.SeedId("vault:happy:owner");
        const string currency = "USD";
        var expectedId = _fix.IdGenerator.Generate($"economic:capital:vault:{ownerId}:{currency}");
        var correlationId = _fix.SeedId("vault:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/vault/create",
            new CreateVaultRequestModel(ownerId, currency), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/vault/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalVaultReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.VaultId);
        Assert.Equal(ownerId, read.Data.OwnerId);
        Assert.Equal(currency, read.Data.Currency);
    }

    [Fact]
    public async Task Failure_AddSliceToUnknownVault_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostVaultId = _fix.SeedId("vault:fail:ghost");
        var correlationId = _fix.SeedId("vault:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/vault/slice/add",
            new AddVaultSliceRequestModel(ghostVaultId, 100m, "USD"), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/vault/{ghostVaultId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostVaultId);
    }
}
