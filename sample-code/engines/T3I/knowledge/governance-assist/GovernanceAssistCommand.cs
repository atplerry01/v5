namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Command to trigger AGA analysis and recommendation generation.
/// </summary>
public sealed record GovernanceAssistCommand
{
    public required Guid AnalysisId { get; init; }
    public string? DomainFilter { get; init; }
    public Guid? PolicyIdFilter { get; init; }
    public bool IncludeOptimization { get; init; } = true;
    public bool IncludeDriftAnalysis { get; init; } = true;
    public bool IncludeAnomalyDetection { get; init; } = true;
    public bool IncludeFederationAnalysis { get; init; } = true;
    public int MaxRiskThreshold { get; init; } = 75;
}

/// <summary>
/// Command to generate a governance proposal draft from an existing recommendation.
/// </summary>
public sealed record ProposalDraftCommand
{
    public required Guid RecommendationId { get; init; }
    public required Guid RequestedBy { get; init; }
}
