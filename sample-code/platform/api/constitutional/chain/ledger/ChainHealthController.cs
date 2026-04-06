using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Constitutional.Chain.Ledger;

public sealed class ChainHealthController
{
    private const string StateProjection = "chain.state";
    private const string HealthProjection = "chain.health";
    private const string AnomalyProjection = "chain.anomaly";

    private readonly ProjectionAdapter _projections;

    public ChainHealthController(ProjectionAdapter projections)
    {
        _projections = projections ?? throw new ArgumentNullException(nameof(projections));
    }

    public async Task<ApiResponse> GetHealthAsync(ApiRequest context)
    {
        return await _projections.QueryAsync<ChainHealthDto>(
            HealthProjection,
            new Dictionary<string, object> { ["key"] = "chain-health" },
            context.TraceId);
    }

    public async Task<ApiResponse> GetHeadAsync(ApiRequest context)
    {
        return await _projections.QueryAsync<ChainStateDto>(
            StateProjection,
            new Dictionary<string, object> { ["key"] = "chain-state" },
            context.TraceId);
    }

    public async Task<ApiResponse> GetAnomaliesAsync(ApiRequest context)
    {
        return await _projections.QueryAsync<ChainAnomalySummaryDto>(
            AnomalyProjection,
            new Dictionary<string, object> { ["key"] = "chain-anomaly-summary" },
            context.TraceId);
    }
}

public sealed record ChainHealthDto
{
    public bool ContinuityValid { get; init; }
    public bool HashConsistent { get; init; }
    public bool IsHealthy { get; init; }
    public long ExpectedNextSequence { get; init; }
    public int MissingSequenceCount { get; init; }
    public int ContinuityBreakCount { get; init; }
    public DateTimeOffset LastChecked { get; init; }
}

public sealed record ChainStateDto
{
    public string? CurrentHeadHash { get; init; }
    public string? CurrentHeadBlockId { get; init; }
    public long BlockHeight { get; init; }
    public DateTimeOffset LastBlockTimestamp { get; init; }
}

public sealed record ChainAnomalySummaryDto
{
    public int TotalAnomalies { get; init; }
    public int HashMismatches { get; init; }
    public int DuplicateCorrelations { get; init; }
    public int ForkAttempts { get; init; }
    public DateTimeOffset LastAnomalyAt { get; init; }
}
