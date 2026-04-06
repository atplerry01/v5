using Microsoft.Extensions.Logging;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;

namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Anchors enforcement results (guard + policy + hashes) to WhyceChain.
/// Runs after execution on both success AND failure paths.
/// FAILURE TO ANCHOR BLOCKS EXECUTION — non-negotiable.
/// </summary>
public sealed class EnforcementChainAnchor
{
    private readonly IChainWriter _chainWriter;
    private readonly ILogger<EnforcementChainAnchor> _logger;
    private readonly EnforcementMetrics? _metrics;

    public EnforcementChainAnchor(
        IChainWriter chainWriter,
        ILogger<EnforcementChainAnchor> logger,
        EnforcementMetrics? metrics = null)
    {
        _chainWriter = chainWriter;
        _logger = logger;
        _metrics = metrics;
    }

    /// <summary>
    /// Anchor enforcement context to WhyceChain. Must be called on both success and failure.
    /// Throws <see cref="EnforcementChainException"/> on failure — execution MUST NOT proceed.
    /// </summary>
    public async Task<ChainWriteResult> AnchorAsync(
        EnforcementContext enforcement,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new ChainPayload
            {
                EventId = $"enforcement:{enforcement.CorrelationId}:{enforcement.Outcome}",
                AggregateId = enforcement.CorrelationId,
                EventType = "EnforcementExecuted",
                EventDataHash = ComputeEnforcementHash(enforcement),
                PolicyDecisionHash = enforcement.DecisionHash,
                ExecutionHash = enforcement.GuardHash,
                OccurredAt = enforcement.Timestamp,
                CorrelationId = enforcement.CorrelationId
            };

            var result = await _chainWriter.WriteAsync(payload, cancellationToken);

            _metrics?.RecordChainAnchor(true, enforcement.ShardId);

            _logger.LogInformation(
                "Enforcement anchored to chain: block={BlockId} seq={Seq} outcome={Outcome} partition={PartitionId} shard={ShardId}",
                result.BlockId, result.SequenceNumber, enforcement.Outcome,
                enforcement.PartitionId, enforcement.ShardId ?? "none");

            return result;
        }
        catch (Exception ex)
        {
            _metrics?.RecordChainAnchor(false, enforcement.ShardId);

            _logger.LogCritical(ex,
                "CRITICAL: Failed to anchor enforcement to chain for correlation={CorrelationId}. Execution BLOCKED.",
                enforcement.CorrelationId);

            throw new EnforcementChainException(
                $"Enforcement chain anchoring failed for correlation={enforcement.CorrelationId}. Execution blocked.",
                ex);
        }
    }

    private static string ComputeEnforcementHash(EnforcementContext ctx)
    {
        var input = $"{ctx.CommandName}:{ctx.CorrelationId}:{ctx.DecisionHash ?? "none"}:{ctx.GuardHash ?? "none"}:{ctx.Outcome}:{ctx.PartitionId}:{ctx.ShardId ?? "none"}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hash);
    }
}
