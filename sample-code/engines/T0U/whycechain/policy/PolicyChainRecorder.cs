using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Shared.Utils;

namespace Whycespace.Engines.T0U.WhyceChain.Policy;

/// <summary>
/// Records policy decisions to WhyceChain as immutable evidence.
/// Append-only — no updates, no deletes.
/// </summary>
public sealed class PolicyChainRecorder : ChainEngineBase, IChainRecorder
{
    private readonly IChainWriter _writer;
    private readonly IClock _clock;

    public PolicyChainRecorder(IChainWriter writer, IClock clock)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<PolicyChainRecord> RecordAsync(
        PolicyEvaluationResult result,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        var decisionHash = PolicyHashGenerator.ComputeDecisionHash(
            result.PolicyIds,
            result.DecisionType,
            result.EvaluationTrace);

        var payload = new ChainPayload
        {
            EventId = DeterministicIdHelper.FromSeed($"PolicyChainEvent:{correlationId}:{result.DecisionType}:{decisionHash}").ToString(),
            AggregateId = correlationId,
            EventType = $"policy.{result.DecisionType}",
            EventDataHash = decisionHash,
            PolicyDecisionHash = decisionHash,
            ExecutionHash = null,
            OccurredAt = result.EventPayload?.EvaluatedAt ?? _clock.UtcNowOffset
        };

        var chainResult = await _writer.WriteAsync(payload, cancellationToken);

        return new PolicyChainRecord(
            chainResult.BlockId,
            chainResult.Hash,
            decisionHash,
            chainResult.PreviousHash,
            chainResult.Timestamp);
    }
}

