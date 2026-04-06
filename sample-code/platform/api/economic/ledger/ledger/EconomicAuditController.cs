using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Economic.Ledger.Ledger;

public sealed class EconomicAuditController
{
    private const string ProjectionName = "economic.ledger-policy-link";
    private readonly ProjectionAdapter _projections;

    public EconomicAuditController(ProjectionAdapter projections)
    {
        _projections = projections ?? throw new ArgumentNullException(nameof(projections));
    }

    public async Task<ApiResponse> GetByAccountIdAsync(string accountId, ApiRequest context)
    {
        return await _projections.QueryAsync<EconomicAuditIndexDto>(
            ProjectionName,
            new Dictionary<string, object> { ["key"] = $"ledger-policy-links-by-account:{accountId}" },
            context.TraceId);
    }

    public async Task<ApiResponse> GetByDecisionHashAsync(string decisionHash, ApiRequest context)
    {
        return await _projections.QueryAsync<EconomicAuditDto>(
            ProjectionName,
            new Dictionary<string, object> { ["key"] = $"ledger-policy-link:{decisionHash}" },
            context.TraceId);
    }
}

public sealed record EconomicAuditDto
{
    public string? DecisionHash { get; init; }
    public string? AccountId { get; init; }
    public string? AssetId { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string? TransactionType { get; init; }
    public string? PolicyId { get; init; }
    public string? Decision { get; init; }
    public string? SubjectId { get; init; }
    public string? BlockId { get; init; }
    public DateTimeOffset AnchoredAt { get; init; }
}

public sealed record EconomicAuditIndexDto
{
    public string? AccountId { get; init; }
    public List<string> DecisionHashes { get; init; } = [];
    public int LinkCount { get; init; }
}
