using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Reserve;

namespace Whycespace.Tests.E2E.Economic.Capital.Reserve;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalReserveE2ETests
{
    private const string ProjSchema = "projection_economic_capital_reserve";
    private const string ProjTable  = "capital_reserve_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalReserveE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateReserve_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var accountId = _fix.SeedId("reserve:happy:account");
        const decimal amount = 75m;
        const string currency = "USD";
        // ExpiresAt is deterministic — derived from the frozen TestClock + a fixed 1-day window.
        var expiresAt = _fix.Clock.UtcNow.AddDays(1);
        var expectedId = _fix.IdGenerator.Generate(
            $"economic:capital:reserve:{accountId}:{amount}:{currency}");
        var correlationId = _fix.SeedId("reserve:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/reserve/create",
            new CreateReserveRequestModel(accountId, amount, currency, expiresAt), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/reserve/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalReserveReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.ReserveId);
        Assert.Equal(accountId, read.Data.AccountId);
        Assert.Equal(amount, read.Data.Amount);
        Assert.Equal(currency, read.Data.Currency);
    }

    [Fact]
    public async Task Failure_ReleaseUnknownReserve_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostId = _fix.SeedId("reserve:fail:ghost");
        var correlationId = _fix.SeedId("reserve:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/reserve/release",
            new ReleaseReserveRequestModel(ghostId), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/reserve/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
