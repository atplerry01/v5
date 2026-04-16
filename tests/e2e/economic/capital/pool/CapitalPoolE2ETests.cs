using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;

namespace Whycespace.Tests.E2E.Economic.Capital.Pool;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalPoolE2ETests
{
    private const string ProjSchema = "projection_economic_capital_pool";
    private const string ProjTable  = "capital_pool_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalPoolE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreatePool_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        // Pool aggregateId is derived from Currency alone (controller seed:
        // "economic:capital:pool:{Currency}"), so we suffix RunId into the
        // currency string to keep pool ids unique across runs.
        var currency = $"USD-{CapitalE2EConfig.RunId}";
        var expectedId = _fix.IdGenerator.Generate($"economic:capital:pool:{currency}");
        var correlationId = _fix.SeedId("pool:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/pool/create",
            new CreatePoolRequestModel(currency), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/pool/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalPoolReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.PoolId);
        Assert.Equal(currency, read.Data.Currency);
    }

    [Fact]
    public async Task Failure_ReduceUnknownPool_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostPoolId      = _fix.SeedId("pool:fail:ghost");
        var ghostSourceAcct  = _fix.SeedId("pool:fail:src");
        var correlationId    = _fix.SeedId("pool:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/pool/reduce",
            new ReduceFromPoolRequestModel(ghostPoolId, ghostSourceAcct, 50m), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/pool/{ghostPoolId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostPoolId);
    }
}
