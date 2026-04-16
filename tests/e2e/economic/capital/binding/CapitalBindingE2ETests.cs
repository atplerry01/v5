using System.Net;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Capital.Binding;

namespace Whycespace.Tests.E2E.Economic.Capital.Binding;

[Collection(CapitalE2ECollection.Name)]
public sealed class CapitalBindingE2ETests
{
    private const string ProjSchema = "projection_economic_capital_binding";
    private const string ProjTable  = "capital_binding_read_model";

    private readonly CapitalE2EFixture _fix;
    public CapitalBindingE2ETests(CapitalE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_BindAccount_EmitsEvent_UpdatesProjection_QueryReturnsState()
    {
        var accountId = _fix.SeedId("binding:happy:account");
        var ownerId   = _fix.SeedId("binding:happy:owner");
        const int ownershipType = 1; // Individual
        var expectedId = _fix.IdGenerator.Generate(
            $"economic:capital:binding:{accountId}:{ownerId}");
        var correlationId = _fix.SeedId("binding:happy:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/binding/bind",
            new BindRequestModel(accountId, ownerId, ownershipType), correlationId);

        Assert.Equal(HttpStatusCode.OK, post.StatusCode);
        var ack = await ApiEnvelope.ReadAsync<CommandAck>(post);
        Assert.True(ack!.Success);

        await ProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedId, CapitalE2EConfig.PollTimeout);

        var get = await _fix.Http.GetAsync($"/api/capital/binding/{expectedId}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var read = await ApiEnvelope.ReadAsync<CapitalBindingReadModel>(get);
        Assert.True(read!.Success);
        Assert.Equal(expectedId, read.Data!.BindingId);
        Assert.Equal(accountId, read.Data.AccountId);
        Assert.Equal(ownerId, read.Data.OwnerId);
        Assert.Equal(ownershipType, read.Data.OwnershipType);
    }

    [Fact]
    public async Task Failure_ReleaseUnknownBinding_ReturnsError_NoEventStored_NoProjection()
    {
        var ghostId = _fix.SeedId("binding:fail:ghost");
        var correlationId = _fix.SeedId("binding:fail:corr");

        var post = await ApiEnvelope.PostAsync(
            _fix.Http, "/api/capital/binding/release",
            new ReleaseBindingRequestModel(ghostId), correlationId);

        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/capital/binding/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await ProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
