using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Retry.PolicyAnchor;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Storage;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Shared.Utils;

namespace Whycespace.Runtime.Chain;

/// <summary>
/// Runtime service that transforms PolicyDecision → AnchorPayload → ChainWrite.
/// Executes AFTER policy evaluation. Non-blocking: failure does NOT fail the main request.
///
/// E4.1: CorrelationId = DecisionHash (no timestamp). Duplicate detection via correlation_id
/// uniqueness constraint. Same logical decision → same correlation → single block.
///
/// Flow: PolicyDecision → AnchorPayload → WhyceChainWriter → Block
/// On failure: emits failure event + schedules retry via PolicyAnchorRetryQueue.
/// </summary>
public sealed class PolicyDecisionAnchorService : IPolicyDecisionAnchor
{
    private readonly IChainWriter _writer;
    private readonly IClock _clock;
    private readonly IEventPublisher? _eventPublisher;
    private readonly PolicyAnchorRetryQueue? _retryQueue;

    public PolicyDecisionAnchorService(
        IChainWriter writer,
        IClock clock,
        IEventPublisher? eventPublisher = null,
        PolicyAnchorRetryQueue? retryQueue = null)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _eventPublisher = eventPublisher;
        _retryQueue = retryQueue;
    }

    public async Task<PolicyAnchorResult> AnchorAsync(PolicyAnchorRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            // E4.1: Compute execution hash (unique per attempt, for audit)
            var executionHash = PolicyAnchorHashService.ComputeExecutionHash(
                request.DecisionHash, request.Timestamp);

            // Build chain payload — CorrelationId = DecisionHash for idempotency
            var payload = new ChainPayload
            {
                EventId = DeterministicIdHelper.FromSeed($"chain-payload:{request.DecisionHash}:{request.CorrelationId}").ToString(),
                AggregateId = request.CorrelationId,
                EventType = $"policy.decision.{request.Decision.ToLowerInvariant()}",
                EventDataHash = request.EvaluationHash,
                PolicyDecisionHash = request.DecisionHash,
                ExecutionHash = executionHash,
                OccurredAt = request.Timestamp,
                CorrelationId = request.CorrelationId
            };

            var result = await _writer.WriteAsync(payload, ct);

            // Emit success event (E5: identity, E6: economic fields)
            await EmitEventAsync(
                "whyce.observability.policy.decision.anchored",
                new
                {
                    request.PolicyId,
                    request.Decision,
                    request.CorrelationId,
                    request.DecisionHash,
                    BlockId = result.BlockId,
                    BlockHash = result.Hash,
                    request.Subject,
                    request.Resource,
                    request.Action,
                    request.SubjectId,
                    request.Roles,
                    request.TrustScore,
                    request.IsVerified,
                    request.SessionId,
                    request.DeviceId,
                    // E6: Economic fields
                    request.AccountId,
                    request.AssetId,
                    request.Amount,
                    request.Currency,
                    request.TransactionType,
                    // E7: Workflow fields
                    request.WorkflowId,
                    request.StepId,
                    request.WorkflowState,
                    request.Transition,
                    AnchoredAt = result.Timestamp
                },
                request.CorrelationId, ct);

            return PolicyAnchorResult.Ok(result.BlockId, result.Hash);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("DUPLICATE_CORRELATION"))
        {
            // E4.1: Duplicate detected — same decision already anchored.
            // This is a silent success (idempotent). Emit observability event.
            await EmitEventAsync(
                "whyce.observability.policy.decision.duplicated",
                new
                {
                    request.PolicyId,
                    request.Decision,
                    request.DecisionHash,
                    request.CorrelationId,
                    DetectedAt = _clock.UtcNowOffset
                },
                request.CorrelationId, ct);

            return PolicyAnchorResult.Duplicate(request.DecisionHash);
        }
        catch (Exception ex)
        {
            // Non-blocking: emit failure event + schedule retry
            await EmitEventAsync(
                "whyce.observability.policy.decision.events",
                new
                {
                    request.PolicyId,
                    request.Decision,
                    request.CorrelationId,
                    Error = ex.Message,
                    FailedAt = _clock.UtcNowOffset
                },
                request.CorrelationId, ct);

            // Retry reuses SAME request (same correlationId = same decisionHash)
            _retryQueue?.Enqueue(request);

            return PolicyAnchorResult.Fail(ex.Message);
        }
    }

    private async Task EmitEventAsync(string eventType, object payload, string correlationId, CancellationToken ct)
    {
        if (_eventPublisher is null) return;

        await _eventPublisher.PublishAsync(new RuntimeEvent
        {
            EventId = DeterministicIdHelper.FromSeed($"anchor-event:{eventType}:{correlationId}"),
                AggregateId = Guid.Empty,
            AggregateType = "WhycePolicy",
            EventType = eventType,
            CorrelationId = correlationId,
            Payload = payload,
            Timestamp = _clock.UtcNowOffset
        }, ct);
    }
}
