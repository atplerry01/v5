namespace Whycespace.Engines.T3I.Atlas.Service;

/// <summary>
/// T3I engine: risk analysis business logic.
/// Extracted from Systems.WhyceAtlas.RiskAnalysisService.
/// </summary>
public sealed class RiskAnalysisEngine
{
    public RiskAssessmentResult Assess(RiskAssessmentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var incidentRate = command.IncidentCount;
        var isCompliant = command.IsCompliant;

        var riskLevel = (incidentRate, isCompliant) switch
        {
            ( > 10, false) => RiskLevel.Critical,
            ( > 10, true) => RiskLevel.High,
            ( > 3, false) => RiskLevel.High,
            ( > 3, true) => RiskLevel.Medium,
            (_, false) => RiskLevel.Medium,
            _ => RiskLevel.Low
        };

        return new RiskAssessmentResult
        {
            ClusterId = command.ClusterId,
            OperationType = command.OperationType,
            Level = riskLevel,
            IncidentCount = incidentRate,
            IsCompliant = isCompliant,
            RecommendAction = riskLevel >= RiskLevel.High ? "Review required before execution" : "Proceed"
        };
    }
}

public sealed record RiskAssessmentCommand
{
    public required string ClusterId { get; init; }
    public required string OperationType { get; init; }
    public required int IncidentCount { get; init; }
    public required bool IsCompliant { get; init; }
}

public sealed record RiskAssessmentResult
{
    public required string ClusterId { get; init; }
    public required string OperationType { get; init; }
    public required RiskLevel Level { get; init; }
    public required int IncidentCount { get; init; }
    public required bool IsCompliant { get; init; }
    public required string RecommendAction { get; init; }
}

public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}
