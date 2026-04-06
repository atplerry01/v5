using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T4A.CoreSystem;

/// <summary>
/// T4A background verification worker. Dispatches core-system validation
/// commands through the runtime on a periodic schedule.
///
/// Responsibilities:
/// - Periodic reconciliation (event store vs projections)
/// - Drift detection (cross-ledger consistency)
/// - Invariant re-check (financial balance, temporal ordering)
///
/// Performance:
/// - Partitioned verification: checks are split across shards
/// - Configurable intervals per check type
/// - Parallel execution of independent check groups
///
/// T4A engines trigger operations via runtime — they do NOT execute
/// domain logic directly. All verification is dispatched as commands.
/// NO persistence, NO domain imports.
/// </summary>
public sealed class CoreSystemVerificationWorker : IEngine
{
    public string EngineId => "coresystem.verification-worker.v1";

    public Task<EngineResult> ExecuteAsync(EngineRequest request, CancellationToken cancellationToken = default)
    {
        var verificationType = request.Headers.TryGetValue("x-verification-type", out var vt) ? vt : "full";
        var partitionId = request.Headers.TryGetValue("x-partition-id", out var pid) ? pid : "0";
        var totalPartitions = request.Headers.TryGetValue("x-total-partitions", out var tp) && int.TryParse(tp, out var tpv) ? tpv : 1;

        var allChecks = BuildCheckList(verificationType);
        var partitionedChecks = PartitionChecks(allChecks, partitionId, totalPartitions);

        var verificationPlan = new VerificationPlan
        {
            VerificationType = verificationType,
            CorrelationId = request.CorrelationId,
            PartitionId = partitionId,
            TotalPartitions = totalPartitions,
            ScheduledChecks = partitionedChecks,
            CheckGroups = GroupChecks(partitionedChecks)
        };

        return Task.FromResult(EngineResult.Ok(verificationPlan));
    }

    private static string[] BuildCheckList(string verificationType) =>
        verificationType switch
        {
            "financial" => ["inflow-outflow-balance", "negative-balance", "vault-consistency"],
            "reconciliation" => ["event-store-projection", "cross-ledger", "spv-financial"],
            "temporal" => ["monotonic-ordering", "schedule-overlap"],
            "state" => ["system-snapshot", "state-coherence"],
            "full" =>
            [
                "inflow-outflow-balance", "negative-balance", "vault-consistency",
                "event-store-projection", "cross-ledger", "spv-financial",
                "monotonic-ordering", "schedule-overlap",
                "system-snapshot", "state-coherence"
            ],
            _ => ["inflow-outflow-balance", "event-store-projection"]
        };

    /// <summary>
    /// Partitions checks across shards using deterministic modulo assignment.
    /// Same partition ID always gets the same subset of checks.
    /// </summary>
    private static string[] PartitionChecks(string[] allChecks, string partitionId, int totalPartitions)
    {
        if (totalPartitions <= 1)
            return allChecks;

        if (!int.TryParse(partitionId, out var pid))
            pid = 0;

        return allChecks
            .Where((_, index) => index % totalPartitions == pid)
            .ToArray();
    }

    /// <summary>
    /// Groups checks by domain for parallel execution.
    /// Independent groups can run concurrently.
    /// </summary>
    private static VerificationCheckGroup[] GroupChecks(string[] checks)
    {
        var groups = new Dictionary<string, List<string>>();
        foreach (var check in checks)
        {
            var domain = check switch
            {
                var c when c.Contains("balance") || c.Contains("vault") => "financial",
                var c when c.Contains("ledger") || c.Contains("projection") || c.Contains("spv") => "reconciliation",
                var c when c.Contains("monotonic") || c.Contains("overlap") => "temporal",
                var c when c.Contains("snapshot") || c.Contains("coherence") => "state",
                _ => "general"
            };
            if (!groups.ContainsKey(domain))
                groups[domain] = [];
            groups[domain].Add(check);
        }
        return groups.Select(g => new VerificationCheckGroup
        {
            Domain = g.Key,
            Checks = g.Value.ToArray()
        }).ToArray();
    }
}

public sealed record VerificationPlan
{
    public required string VerificationType { get; init; }
    public required string CorrelationId { get; init; }
    public required string PartitionId { get; init; }
    public required int TotalPartitions { get; init; }
    public required string[] ScheduledChecks { get; init; }
    public required VerificationCheckGroup[] CheckGroups { get; init; }
}

public sealed record VerificationCheckGroup
{
    public required string Domain { get; init; }
    public required string[] Checks { get; init; }
}

/// <summary>
/// Configuration for the verification worker's scheduling behavior.
/// Passed via runtime configuration — NOT hardcoded.
/// </summary>
public sealed record VerificationScheduleConfig
{
    public TimeSpan FinancialInterval { get; init; } = TimeSpan.FromMinutes(5);
    public TimeSpan ReconciliationInterval { get; init; } = TimeSpan.FromMinutes(10);
    public TimeSpan TemporalInterval { get; init; } = TimeSpan.FromMinutes(15);
    public TimeSpan StateInterval { get; init; } = TimeSpan.FromMinutes(30);
    public int TotalPartitions { get; init; } = 4;
}
