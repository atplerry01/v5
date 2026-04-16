using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Allocation;

namespace Whycespace.Tests.E2E.Economic.Capital.Allocation;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalAllocationE2ETests
{
    private const string ProjSchema = "projection_economic_capital_allocation";
    private const string ProjTable  = "capital_allocation_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalAllocationE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAllocation_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var sourceAccountId = _fix.SeedId("allocation:happy:source");
        var targetId        = _fix.SeedId("allocation:happy:target");
        const decimal amount = 250m;
        const string currency = "USD";
        var expectedId = _fix.IdGenerator.Generate(
            $"economic:capital:allocation:{sourceAccountId}:{targetId}:{amount}");
        var correlationId = _fix.SeedId("allocation:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/allocation/create",
            new CreateAllocationRequestModel(sourceAccountId, targetId, amount, currency), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/allocation/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalAllocationReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.AllocationId);
        Assert.Equal(sourceAccountId, read.Data.SourceAccountId);
        Assert.Equal(targetId, read.Data.TargetId);
        Assert.Equal(amount, read.Data.Amount);
    }

    [Fact]
    public async Task Failure_ReleaseUnknownAllocation_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostId = _fix.SeedId("allocation:fail:ghost");
        var correlationId = _fix.SeedId("allocation:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/allocation/release",
            new ReleaseAllocationRequestModel(ghostId), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/allocation/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
