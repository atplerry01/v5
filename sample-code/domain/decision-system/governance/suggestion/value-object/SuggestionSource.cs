namespace Whycespace.Domain.DecisionSystem.Governance.Suggestion;

/// <summary>
/// Origin of a governance suggestion. Tracks which intelligence engine
/// or analysis produced the suggestion.
/// </summary>
public sealed record SuggestionSource
{
    public string EngineId { get; }
    public string AnalysisId { get; }
    public string Category { get; }

    private SuggestionSource(string engineId, string analysisId, string category)
    {
        EngineId = engineId;
        AnalysisId = analysisId;
        Category = category;
    }

    public static SuggestionSource FromIntelligence(string engineId, string analysisId, string category) =>
        new(engineId, analysisId, category);
}
