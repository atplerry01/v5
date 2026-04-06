namespace Whycespace.Domain.IntelligenceSystem.Economic.Simulation;

public sealed record SimulationResult
{
    public decimal OutcomeValue { get; }
    public decimal Variance { get; }
    public string Summary { get; }
    public string ExpectedOutcome { get; }
    public ConfidenceScore ConfidenceScore { get; }

    public SimulationResult(
        decimal outcomeValue,
        decimal variance,
        string summary,
        string expectedOutcome,
        ConfidenceScore confidenceScore)
    {
        Guard.AgainstEmpty(summary);
        Guard.AgainstEmpty(expectedOutcome);
        Guard.AgainstNull(confidenceScore);
        OutcomeValue = outcomeValue;
        Variance = variance;
        Summary = summary;
        ExpectedOutcome = expectedOutcome;
        ConfidenceScore = confidenceScore;
    }
}
