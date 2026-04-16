using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Asset;

namespace Whycespace.Tests.E2E.Economic.Capital.Asset;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalAssetE2ETests
{
    private const string ProjSchema = "projection_economic_capital_asset";
    private const string ProjTable  = "capital_asset_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalAssetE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAsset_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var ownerId = _fix.SeedId("asset:happy:owner");
        const decimal initialValue = 1500m;
        const string currency = "USD";
        var expectedId = _fix.IdGenerator.Generate(
            $"economic:capital:asset:{ownerId}:{initialValue}:{currency}");
        var correlationId = _fix.SeedId("asset:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/asset/create",
            new CreateAssetRequestModel(ownerId, initialValue, currency), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/asset/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalAssetReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.AssetId);
        Assert.Equal(ownerId, read.Data.OwnerId);
        Assert.Equal(initialValue, read.Data.Value);
        Assert.Equal(currency, read.Data.Currency);
    }

    [Fact]
    public async Task Failure_RevalueUnknownAsset_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostId = _fix.SeedId("asset:fail:ghost");
        var correlationId = _fix.SeedId("asset:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/asset/revalue",
            new RevalueAssetRequestModel(ghostId, 9999m), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/asset/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
