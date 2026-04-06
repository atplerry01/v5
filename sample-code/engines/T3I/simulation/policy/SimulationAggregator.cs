namespace Whycespace.Engines.T3I.PolicySimulation.Aggregator;

/// <summary>
/// Aggregates results from multiple simulation runs.
/// Produces: mean, variance, confidence interval, most frequent decision.
/// Deterministic when all runs use the same snapshot + sequential seeds.
/// </summary>
public sealed class SimulationAggregator
{
    private readonly PolicySimulationEngine _engine;

    public SimulationAggregator(PolicySimulationEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public async Task<AggregatedSimulationResult> AggregateAsync(
        PolicySimulationCommand command,
        int runCount,
        int? baseSeed,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (runCount < 1) throw new ArgumentOutOfRangeException(nameof(runCount), "Must be >= 1.");

        var results = new List<PolicySimulationResult>(runCount);

        for (var i = 0; i < runCount; i++)
        {
            var runCommand = command with
            {
                Seed = baseSeed.HasValue ? baseSeed.Value + i : null,
                RunCount = 1 // Each individual run is a single execution
            };

            var result = await _engine.SimulateAsync(runCommand, cancellationToken);
            results.Add(result);
        }

        return Aggregate(results);
    }

    public static AggregatedSimulationResult Aggregate(IReadOnlyList<PolicySimulationResult> results)
    {
        if (results.Count == 0)
            throw new ArgumentException("At least one result is required.", nameof(results));

        var passedValues = results.Select(r => (double)r.DecisionSummary.RulesPassed).ToList();
        var failedValues = results.Select(r => (double)r.DecisionSummary.RulesFailed).ToList();

        var meanPassed = passedValues.Average();
        var meanFailed = failedValues.Average();

        // Variance of passed rules
        var variance = passedValues.Count > 1
            ? passedValues.Sum(v => Math.Pow(v - meanPassed, 2)) / (passedValues.Count - 1)
            : 0.0;

        // 95% confidence interval using normal approximation
        var stdDev = Math.Sqrt(variance);
        var marginOfError = 1.96 * stdDev / Math.Sqrt(results.Count);
        var ciLow = Math.Max(0, meanPassed - marginOfError);
        var ciHigh = meanPassed + marginOfError;

        // Most frequent overall decision
        var mostFrequent = results
            .GroupBy(r => r.DecisionSummary.OverallDecision)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;

        return new AggregatedSimulationResult(
            results.Count,
            Math.Round(meanPassed, 2),
            Math.Round(meanFailed, 2),
            Math.Round(variance, 4),
            Math.Round(ciLow, 2),
            Math.Round(ciHigh, 2),
            mostFrequent);
    }
}
