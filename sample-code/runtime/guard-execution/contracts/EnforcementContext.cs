using Whycespace.Runtime.ControlPlane.Policy;
using Whycespace.Runtime.GuardExecution.Engine;
using Whycespace.Runtime.GuardExecution.Runtime;

namespace Whycespace.Runtime.GuardExecution.Contracts;

/// <summary>
/// Unified enforcement context binding Guard + Policy + Chain.
/// Carries the complete enforcement state through the pipeline.
///
/// Immutable by convention — all init-only properties except Outcome
/// (mutated on chain failure path). Use With* methods for safe copies.
/// </summary>
public sealed class EnforcementContext
{
    public required string CommandName { get; init; }
    public required object Payload { get; init; }
    public required string CorrelationId { get; init; }
    public string? CausationId { get; init; }

    // Guard results (dual-phase)
    public GuardExecutionReport? PrePolicyReport { get; init; }
    public GuardExecutionReport? PostPolicyReport { get; init; }

    // Policy decision
    public PolicyDecision? PolicyDecision { get; init; }

    // Hashes for chain anchoring
    public string? DecisionHash { get; init; }
    public string? GuardHash { get; init; }

    // Enforcement outcome — mutable for BlockedByChain path
    public required DateTimeOffset Timestamp { get; init; }
    public EnforcementOutcome Outcome { get; set; } = EnforcementOutcome.Pending;

    // Execution mode (Live or Replay)
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.Live;

    // Failure classification (null when no failure)
    public EnforcementFailureType? FailureType { get; init; }

    // Distributed partition routing (deterministic)
    public string? PartitionKey { get; init; }
    public int PartitionId { get; init; }
    public string? ShardId { get; init; }

    public bool IsFullyValidated =>
        PrePolicyReport is { Status: GuardExecutionStatus.Pass or GuardExecutionStatus.Warn }
        && PolicyDecision is { Result: PolicyDecisionResult.Allow }
        && PostPolicyReport is { Status: GuardExecutionStatus.Pass or GuardExecutionStatus.Warn };

    // --- Safe copy methods for deterministic propagation ---

    public EnforcementContext WithDecisionHash(string decisionHash)
        => Copy(overrideDecisionHash: decisionHash);

    public EnforcementContext WithGuardHash(string guardHash)
        => Copy(overrideGuardHash: guardHash);

    public EnforcementContext WithExecutionMode(ExecutionMode mode)
        => Copy(overrideExecutionMode: mode);

    public EnforcementContext WithFailureType(EnforcementFailureType failureType)
        => Copy(overrideFailureType: failureType);

    public EnforcementContext WithPartition(string partitionKey, int partitionId, string shardId)
        => Copy(overridePartitionKey: partitionKey, overridePartitionId: partitionId, overrideShardId: shardId);

    /// <summary>
    /// Creates an EnforcementSignature for logging, telemetry, and chain anchoring.
    /// </summary>
    public EnforcementSignature ToSignature()
        => new()
        {
            CorrelationId = CorrelationId,
            DecisionHash = DecisionHash ?? "none",
            GuardHash = GuardHash ?? "none"
        };

    public const string ContextKey = "Enforcement.Context";

    private EnforcementContext Copy(
        string? overrideDecisionHash = null,
        string? overrideGuardHash = null,
        ExecutionMode? overrideExecutionMode = null,
        EnforcementFailureType? overrideFailureType = null,
        string? overridePartitionKey = null,
        int? overridePartitionId = null,
        string? overrideShardId = null)
    {
        return new EnforcementContext
        {
            CommandName = CommandName,
            Payload = Payload,
            CorrelationId = CorrelationId,
            CausationId = CausationId,
            PrePolicyReport = PrePolicyReport,
            PostPolicyReport = PostPolicyReport,
            PolicyDecision = PolicyDecision,
            DecisionHash = overrideDecisionHash ?? DecisionHash,
            GuardHash = overrideGuardHash ?? GuardHash,
            Timestamp = Timestamp,
            Outcome = Outcome,
            ExecutionMode = overrideExecutionMode ?? ExecutionMode,
            FailureType = overrideFailureType ?? FailureType,
            PartitionKey = overridePartitionKey ?? PartitionKey,
            PartitionId = overridePartitionId ?? PartitionId,
            ShardId = overrideShardId ?? ShardId
        };
    }
}

public enum EnforcementOutcome
{
    Pending,
    Executed,
    BlockedByGuard,
    BlockedByPolicy,
    BlockedByChain
}
