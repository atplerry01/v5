namespace Whycespace.Engines.T3I.Knowledge.CrossCluster;

/// <summary>
/// Analyzes patterns across multiple clusters to detect system-wide trends.
/// Stateless computation engine — no persistence, no external calls.
/// </summary>
public sealed class CrossClusterIntelligenceEngine
{
    /// <summary>
    /// Analyzes cross-cluster health patterns and detects systemic issues.
    /// </summary>
    public CrossClusterAnalysis Analyze(CrossClusterAnalysisCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var findings = new List<CrossClusterFinding>();

        // Detect correlated failures across clusters
        var failingClusters = command.ClusterSnapshots
            .Where(c => c.ErrorRate > 0.1m)
            .ToList();

        if (failingClusters.Count > 1)
        {
            var correlationStrength = (decimal)failingClusters.Count / command.ClusterSnapshots.Count;
            findings.Add(new CrossClusterFinding(
                Type: FindingTypes.CorrelatedFailure,
                Description: $"{failingClusters.Count}/{command.ClusterSnapshots.Count} clusters showing elevated error rates — possible systemic issue",
                Severity: correlationStrength > 0.5m ? "Critical" : "High",
                AffectedClusters: failingClusters.Select(c => c.ClusterId).ToList(),
                Confidence: Math.Min(0.95m, 0.6m + correlationStrength * 0.3m)));
        }

        // Detect resource imbalance across clusters
        if (command.ClusterSnapshots.Count >= 2)
        {
            var avgUtilization = command.ClusterSnapshots.Average(c => c.CapitalUtilization);
            var maxUtil = command.ClusterSnapshots.Max(c => c.CapitalUtilization);
            var minUtil = command.ClusterSnapshots.Min(c => c.CapitalUtilization);

            if (maxUtil - minUtil > 0.4m)
            {
                var overloaded = command.ClusterSnapshots.Where(c => c.CapitalUtilization > avgUtilization + 0.2m).ToList();
                var underloaded = command.ClusterSnapshots.Where(c => c.CapitalUtilization < avgUtilization - 0.2m).ToList();

                findings.Add(new CrossClusterFinding(
                    Type: FindingTypes.ResourceImbalance,
                    Description: $"Capital utilization spread: {minUtil:P0} to {maxUtil:P0}. Consider rebalancing.",
                    Severity: maxUtil > 0.9m ? "High" : "Medium",
                    AffectedClusters: overloaded.Concat(underloaded).Select(c => c.ClusterId).ToList(),
                    Confidence: 0.85m));
            }
        }

        // Detect cascade risk (downstream clusters of failing clusters)
        foreach (var cluster in failingClusters)
        {
            var dependents = command.ClusterSnapshots
                .Where(c => c.DependsOnClusterIds.Contains(cluster.ClusterId))
                .ToList();

            if (dependents.Count > 0)
            {
                findings.Add(new CrossClusterFinding(
                    Type: FindingTypes.CascadeRisk,
                    Description: $"Cluster {cluster.ClusterId} failing with {dependents.Count} dependent clusters at risk",
                    Severity: "Critical",
                    AffectedClusters: dependents.Select(c => c.ClusterId).Prepend(cluster.ClusterId).ToList(),
                    Confidence: 0.8m));
            }
        }

        var overallHealth = findings.Count switch
        {
            0 => "Healthy",
            var n when findings.Any(f => f.Severity == "Critical") => "Critical",
            var n when n > 2 => "Degraded",
            _ => "Warning"
        };

        return new CrossClusterAnalysis(overallHealth, findings);
    }

    private static class FindingTypes
    {
        public const string CorrelatedFailure = "CORRELATED_FAILURE";
        public const string ResourceImbalance = "RESOURCE_IMBALANCE";
        public const string CascadeRisk = "CASCADE_RISK";
    }
}

public sealed record CrossClusterAnalysisCommand(
    IReadOnlyList<ClusterHealthSnapshot> ClusterSnapshots);

public sealed record ClusterHealthSnapshot(
    Guid ClusterId,
    decimal ErrorRate,
    decimal CapitalUtilization,
    int ActiveWorkflows,
    IReadOnlyList<Guid> DependsOnClusterIds);

public sealed record CrossClusterAnalysis(
    string OverallHealth,
    IReadOnlyList<CrossClusterFinding> Findings);

public sealed record CrossClusterFinding(
    string Type,
    string Description,
    string Severity,
    IReadOnlyList<Guid> AffectedClusters,
    decimal Confidence);
