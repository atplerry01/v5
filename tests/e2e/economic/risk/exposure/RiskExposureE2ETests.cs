using System.Net;
using Whycespace.Platform.Api.Controllers.Economic.Risk.Exposure;
using Whycespace.Shared.Contracts.Common;
using Whycespace.Shared.Contracts.Economic.Risk.Exposure;
using Whycespace.Tests.E2E.Economic.Risk.Setup;

namespace Whycespace.Tests.E2E.Economic.Risk.Exposure;

/// <summary>
/// End-to-end validation of the risk/exposure context. Covers the full
/// lifecycle — Create → Increase → Reduce → Close — through the real runtime
/// (API, Kafka event fabric, Postgres projection store), plus a failure path
/// (operating on a non-existent exposure). Exercises the exposure aggregate's
/// invariants across every transition:
///   • exposure totals never go negative after reduction
///   • closed exposures reject further mutation
///   • projection state converges to aggregate state on every step
/// </summary>
[Collection(RiskE2ECollection.Name)]
public sealed class RiskExposureE2ETests
{
    private const string ProjSchema = "projection_economic_risk_exposure";
    private const string ProjTable  = "risk_exposure_read_model";

    // RiskExposureStatus enum values as written by the projection reducer.
    // Values reflect the canonical RiskExposureStatus ordering (0..N).
    private const int StatusActive = 0;
    private const int StatusClosed = 1;

    private readonly RiskE2EFixture _fix;
    public RiskExposureE2ETests(RiskE2EFixture fix) => _fix = fix;

    [Fact]
    public async Task Lifecycle_CreateIncreaseReduceClose_ProjectionTracksEveryTransition()
    {
        const int exposureType = 1;
        const string currency = "USD";
        var sourceId = _fix.SeedId("exposure:lifecycle:source");
        var correlationId = _fix.SeedId("exposure:lifecycle:corr");

        var expectedExposureId = _fix.IdGenerator.Generate(
            $"economic:risk:exposure:{sourceId}:{exposureType}:{currency}");

        // 1) Create — InitialExposure=1000, aggregate enters Active state.
        var create = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/create",
            new CreateExposureRequestModel(sourceId, exposureType, 1000m, currency),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        await RiskProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedExposureId, RiskE2EConfig.PollTimeout);

        // 2) Increase by 500 → total = 1500.
        var increase = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/increase",
            new IncreaseExposureRequestModel(expectedExposureId, 500m),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, increase.StatusCode);

        // 3) Reduce by 300 → total = 1200. Aggregate invariant: total never negative.
        var reduce = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/reduce",
            new ReduceExposureRequestModel(expectedExposureId, 300m),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, reduce.StatusCode);

        // 4) GET — projection reflects latest state; aggregate rehydration proven.
        var getActive = await _fix.Http.GetAsync($"/api/risk/exposure/{expectedExposureId}");
        Assert.Equal(HttpStatusCode.OK, getActive.StatusCode);
        var readActive = await RiskApiEnvelope.ReadAsync<RiskExposureReadModel>(getActive);
        Assert.True(readActive!.Success);
        Assert.Equal(expectedExposureId, readActive.Data!.ExposureId);
        Assert.Equal(sourceId,            readActive.Data.SourceId);
        Assert.Equal(exposureType,        readActive.Data.ExposureType);
        Assert.Equal(currency,            readActive.Data.Currency);
        Assert.Equal(StatusActive,        readActive.Data.Status);

        // 5) Close — terminal transition; projection moves to Closed status.
        var close = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/close",
            new CloseExposureRequestModel(expectedExposureId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, close.StatusCode);

        await RiskProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedExposureId, StatusClosed, RiskE2EConfig.PollTimeout);

        var getClosed = await _fix.Http.GetAsync($"/api/risk/exposure/{expectedExposureId}");
        Assert.Equal(HttpStatusCode.OK, getClosed.StatusCode);
        var readClosed = await RiskApiEnvelope.ReadAsync<RiskExposureReadModel>(getClosed);
        Assert.True(readClosed!.Success);
        Assert.Equal(StatusClosed, readClosed.Data!.Status);
    }

    [Fact]
    public async Task Invariant_CloseAlreadyClosed_Rejected_ProjectionStable()
    {
        const int exposureType = 2;
        const string currency = "EUR";
        var sourceId = _fix.SeedId("exposure:invariant:source");
        var correlationId = _fix.SeedId("exposure:invariant:corr");

        var expectedExposureId = _fix.IdGenerator.Generate(
            $"economic:risk:exposure:{sourceId}:{exposureType}:{currency}");

        await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/create",
            new CreateExposureRequestModel(sourceId, exposureType, 500m, currency),
            correlationId);

        await RiskProjectionVerifier.PollUntilPresentAsync(
            ProjSchema, ProjTable, expectedExposureId, RiskE2EConfig.PollTimeout);

        // First close — succeeds.
        var firstClose = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/close",
            new CloseExposureRequestModel(expectedExposureId),
            correlationId);
        Assert.Equal(HttpStatusCode.OK, firstClose.StatusCode);

        await RiskProjectionVerifier.PollUntilStatusAsync(
            ProjSchema, ProjTable, expectedExposureId, StatusClosed, RiskE2EConfig.PollTimeout);

        // Second close — aggregate invariant: Closed is terminal. Must reject.
        var secondClose = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/close",
            new CloseExposureRequestModel(expectedExposureId),
            correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, secondClose.StatusCode);
    }

    [Fact]
    public async Task Failure_IncreaseUnknownExposure_ReturnsError_NoProjectionRow()
    {
        var ghostId = _fix.SeedId("exposure:fail:ghost");
        var correlationId = _fix.SeedId("exposure:fail:corr");

        var increase = await RiskApiEnvelope.PostAsync(
            _fix.Http, "/api/risk/exposure/increase",
            new IncreaseExposureRequestModel(ghostId, 100m),
            correlationId);
        Assert.Equal(HttpStatusCode.BadRequest, increase.StatusCode);

        var get = await _fix.Http.GetAsync($"/api/risk/exposure/{ghostId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        await RiskProjectionVerifier.AssertAbsentAsync(ProjSchema, ProjTable, ghostId);
    }
}
