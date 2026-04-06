using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Federation;

/// <summary>
/// Federation staged rollout — composition only.
/// Manages the enablement sequence for regions, validates replication
/// readiness, and supports rollback. NO execution, NO domain mutation.
///
/// Boundary declaration:
/// - BCs touched: operational-system/deployment/activation
/// - Engines composed: none directly — dispatches via runtime
/// - Runtime pipelines: region activation + policy middleware
/// </summary>
public sealed class StagedRolloutService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    /// <summary>
    /// Static rollout sequence: ordered list of regions to activate.
    /// Phase 1: UK → Phase 2: EU → Phase 3: US → Phase 4: Global
    /// </summary>
    private static readonly IReadOnlyList<RolloutPhase> RolloutSequence =
    [
        new("phase-1", "eu-west", "UK", 1),
        new("phase-2", "eu-west", "EU", 2),
        new("phase-3", "us-east", "US", 3),
        new("phase-4", "ap-south", "IN", 4),
        new("phase-5", "af-south", "NG", 5),
    ];

    public StagedRolloutService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    public IReadOnlyList<RolloutPhase> GetRolloutSequence() => RolloutSequence;

    /// <summary>
    /// Activates a specific rollout phase by dispatching canary activation.
    /// </summary>
    public async Task<IntentResult> ActivatePhaseAsync(
        string phaseId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var phase = RolloutSequence.FirstOrDefault(p => p.PhaseId == phaseId);
        if (phase is null)
            return IntentResult.Fail(Guid.Empty, $"Unknown phase: {phaseId}", "UNKNOWN_PHASE");

        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.activation.start-canary",
            Payload = new { RegionId = phase.RegionId, JurisdictionCode = phase.JurisdictionCode },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string>
            {
                ["x-target-region"] = phase.RegionId,
                ["x-rollout-phase"] = phase.PhaseId
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Triggers rollback for a region by dispatching a halt command.
    /// </summary>
    public async Task<IntentResult> RollbackAsync(
        string regionId,
        string reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "operational.deployment.activation.halt",
            Payload = new { RegionId = regionId, Reason = reason },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string>
            {
                ["x-target-region"] = regionId,
                ["x-rollback"] = "true"
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Dispatches replication validation for a region before promotion.
    /// </summary>
    public async Task<IntentResult> ValidateReplicationAsync(
        string regionId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "coresystem.reconciliation.validate-replication",
            Payload = new { RegionId = regionId },
            CorrelationId = correlationId,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = regionId }
        }, cancellationToken);
    }
}

public sealed record RolloutPhase(string PhaseId, string RegionId, string JurisdictionCode, int Order);
