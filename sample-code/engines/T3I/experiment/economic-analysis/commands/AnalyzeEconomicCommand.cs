namespace Whycespace.Engines.T3I.EconomicAnalysis;

public sealed record AnalyzeEconomicCommand
{
    public required string AnalysisId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required decimal TotalCredits { get; init; }
    public required decimal TotalDebits { get; init; }
    public required int TransactionCount { get; init; }
}
