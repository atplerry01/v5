using System.Net;
using Whycespace.Platform.Api.Controllers.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderCoverage;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Tests.E2E.Business.Setup;

namespace Whycespace.Tests.E2E.Business.Provider.ProviderScope.ProviderCoverage;

/// <summary>
/// E2E smoke test for the business/provider/provider-scope/provider-coverage
/// vertical. Uses <see cref="BusinessE2EFixture"/> for health-probe, seeded ID
/// generation, wall-clock IClock, and projection verification.
///
/// Requires a running API host, Kafka broker, and the
/// <c>projection_business_provider_provider_coverage.provider_coverage_read_model</c>
/// table provisioned in Postgres.
/// </summary>
[Collection(BusinessE2ECollection.Name)]
public sealed class ProviderCoverageE2ETests
{
    private const string ProjSchema = "projection_business_provider_provider_coverage";
    private const string ProjTable  = "provider_coverage_read_model";

    private const string ScopeKind       = "Region";
    private const string ScopeDescriptor = "us-west-2";

    private readonly BusinessE2EFixture _fix;
    public ProviderCoverageE2ETests(BusinessE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task HappyPath_CreateAddScopeActivate_UpdatesProjection_GetReturnsActiveWithScope()
    {
        var providerCoverageId = _fix.SeedId("provider-coverage:happy:id");
        var providerId         = _fix.SeedId("provider-coverage:happy:provider");
        var corrCreate         = _fix.SeedId("provider-coverage:happy:corr:create");
        var corrAddScope       = _fix.SeedId("provider-coverage:happy:corr:add-scope");
        var corrActivate       = _fix.SeedId("provider-coverage:happy:corr:activate");

        // 1) Create
        var createResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-scope/provider-coverage/create",
            new CreateProviderCoverageRequestModel(providerCoverageId, providerId),
            corrCreate);
        Assert.Equal(HttpStatusCode.OK, createResp.StatusCode);
        var createAck = await ApiEnvelope.ReadAsync<CommandAck>(createResp);
        Assert.NotNull(createAck);
        Assert.True(createAck!.Success);

        await BusinessProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, providerCoverageId, BusinessE2EConfig.PollTimeout);

        // 2) AddScope — satisfies the "at least one scope" invariant before activation.
        var addResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-scope/provider-coverage/add-scope",
            new CoverageScopeMutationRequestModel(providerCoverageId, ScopeKind, ScopeDescriptor),
            corrAddScope);
        Assert.Equal(HttpStatusCode.OK, addResp.StatusCode);
        var addAck = await ApiEnvelope.ReadAsync<CommandAck>(addResp);
        Assert.NotNull(addAck);
        Assert.True(addAck!.Success);

        // 3) Activate
        var activateResp = await ApiEnvelope.PostAsync(
            _fix.Http,
            "/api/provider-scope/provider-coverage/activate",
            new ProviderCoverageIdRequestModel(providerCoverageId),
            corrActivate);
        Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);
        var activateAck = await ApiEnvelope.ReadAsync<CommandAck>(activateResp);
        Assert.NotNull(activateAck);
        Assert.True(activateAck!.Success);

        // 4) GET projection — poll until Status == "Active" AND the added scope is present.
        ProviderCoverageReadModel? read = null;
        var deadline = DateTime.UtcNow + BusinessE2EConfig.PollTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var get = await _fix.Http.GetAsync($"/api/provider-scope/provider-coverage/{providerCoverageId}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            var payload = await ApiEnvelope.ReadAsync<ProviderCoverageReadModel>(get);
            Assert.NotNull(payload);
            Assert.True(payload!.Success);
            read = payload.Data;
            if (read is not null
                && read.Status == "Active"
                && read.Scopes.Any(s => s.ScopeKind == ScopeKind && s.ScopeDescriptor == ScopeDescriptor))
            {
                break;
            }
            await Task.Delay(200);
        }

        Assert.NotNull(read);
        Assert.Equal(providerCoverageId, read!.ProviderCoverageId);
        Assert.Equal("Active", read.Status);
        Assert.Contains(read.Scopes, s => s.ScopeKind == ScopeKind && s.ScopeDescriptor == ScopeDescriptor);
    }
}
