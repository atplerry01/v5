using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Account;

namespace Whycespace.Tests.E2E.Economic.Capital.Account;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalAccountE2ETests
{
    private const string ProjSchema = "projection_economic_capital_account";
    private const string ProjTable  = "capital_account_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalAccountE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_OpenAccount_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var ownerId = _fix.SeedId("account:happy:owner");
        const string currency = "USD";
        var expectedAccountId = _fix.IdGenerator.Generate($"economic:capital:account:{ownerId}:{currency}");
        var correlationId = _fix.SeedId("account:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/account/open",
            new OpenAccountRequestModel(ownerId, currency), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.NotNull(ack);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedAccountId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/account/{expectedAccountId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalAccountReadModel>(get);
        Assert.NotNull(read);
        Assert.True(read!.Success);
        Assert.Equal(expectedAccountId, read.Data!.AccountId);
        Assert.Equal(ownerId, read.Data.OwnerId);
        Assert.Equal(currency, read.Data.Currency);
    }

    [Fact]
    public async Task Failure_CreditUnknownAccount_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostAccountId = _fix.SeedId("account:fail:ghost");
        var correlationId = _fix.SeedId("account:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/account/credit",
            new CreditAccountRequestModel(ghostAccountId, 100m, "USD"), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/account/{ghostAccountId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostAccountId);
    }
}
