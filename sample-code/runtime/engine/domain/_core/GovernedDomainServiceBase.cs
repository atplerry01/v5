using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Domain;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Shared.Utils;
using Whycespace.Runtime.Observability;

namespace Whycespace.Runtime.Engine.Domain;

/// <summary>
/// Base class for all governed domain services.
/// Enforces: policy evaluation -> execution -> chain anchoring -> observability.
/// No execution path bypasses policy or chain.
/// </summary>
public abstract class GovernedDomainServiceBase
{
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IPolicyDecisionAnchor _chainAnchor;
    private readonly EnforcementMetrics _metrics;
    private readonly EnforcementAnomalyEmitter _anomalyEmitter;
    private readonly IClock _clock;

    protected GovernedDomainServiceBase(
        IPolicyEvaluator policyEvaluator,
        IPolicyDecisionAnchor chainAnchor,
        EnforcementMetrics metrics,
        EnforcementAnomalyEmitter anomalyEmitter,
        IClock clock)
    {
        _policyEvaluator = policyEvaluator;
        _chainAnchor = chainAnchor;
        _metrics = metrics;
        _anomalyEmitter = anomalyEmitter;
        _clock = clock;
    }

    /// <summary>
    /// Executes a domain operation under full governance:
    /// 1. Validate execution context
    /// 2. Evaluate policy
    /// 3. Execute domain operation
    /// 4. Anchor decision to chain
    /// 5. Record metrics
    /// </summary>
    protected Task<DomainResult<T>> ExecuteGovernedAsync<T>(
        DomainExecutionContext context,
        Func<Task<T>> operation,
        CancellationToken ct = default)
        => ExecuteGovernedAsync(context, operation, domainPolicyContext: null, ct);

    protected async Task<DomainResult<T>> ExecuteGovernedAsync<T>(
        DomainExecutionContext context,
        Func<Task<T>> operation,
        object? domainPolicyContext,
        CancellationToken ct = default)
    {
        var startTime = _clock.UtcNowOffset;

        // 1. Validate enforcement context — blocks unguarded execution
        context.Validate();

        // 2. Evaluate policy (or bypass with NoPolicyFlag for read-only operations)
        PolicyEvaluationResult policyResult;

        if (context.NoPolicyFlag)
        {
            // Explicit bypass — emit anomaly signal for audit trail
            _anomalyEmitter.Emit(new EnforcementAnomalySignal
            {
                Type = "NO_POLICY_FLAG_USED",
                CorrelationId = context.CorrelationId,
                Description = $"Policy evaluation bypassed via NoPolicyFlag for {context.Action} on {context.Domain}",
                CommandType = context.CommandType ?? context.Action,
                Timestamp = _clock.UtcNowOffset
            });
            _metrics.RecordPolicyDecision("NO_POLICY", context.CommandType ?? context.Action);

            policyResult = PolicyEvaluationResult.Compliant();
        }
        else
        {
            Guid.TryParse(context.PolicyId, out var policyGuid);
            Guid.TryParse(context.ActorId, out var actorGuid);

            var policyInput = new PolicyEvaluationInput(
                PolicyId: policyGuid == Guid.Empty ? null : policyGuid,
                ActorId: actorGuid,
                Action: context.Action,
                Resource: context.Domain,
                Environment: "runtime",
                Timestamp: context.Timestamp)
            {
                DomainContext = domainPolicyContext
            };

            try
            {
                policyResult = await _policyEvaluator.EvaluateAsync(policyInput, ct);
            }
            catch (Exception ex)
            {
                _metrics.RecordPolicyDecision("error", context.CommandType ?? context.Action);
                _anomalyEmitter.Emit(new EnforcementAnomalySignal
                {
                    Type = "POLICY_EVALUATION_FAILURE",
                    CorrelationId = context.CorrelationId,
                    Description = $"Policy evaluation failed: {ex.Message}",
                    CommandType = context.CommandType ?? context.Action,
                    Timestamp = _clock.UtcNowOffset
                });
                return DomainResult<T>.Fail(new DomainError("POLICY_EVALUATION_FAILED", ex.Message));
            }

            _metrics.RecordPolicyDecision(policyResult.DecisionType, context.CommandType ?? context.Action);

            if (!policyResult.IsCompliant)
            {
                var reason = policyResult.Violations.Count > 0
                    ? string.Join("; ", policyResult.Violations)
                    : "Policy denied";
                return DomainResult<T>.PolicyDenied(reason, context.PolicyId);
            }
        }

        // 3. Execute domain operation
        T data;
        try
        {
            data = await operation();
        }
        catch (Exception ex)
        {
            _metrics.RecordEnforcementOutcome("execution_failed", context.CommandType ?? context.Action, 0);
            return DomainResult<T>.Fail(new DomainError("EXECUTION_FAILED", ex.Message));
        }

        // 4. Compute hashes for chain anchoring
        var contextHash = context.PayloadHash ?? context.CorrelationId;
        var decisionHash = PolicyAnchorHashService.ComputeDecisionHash(
            policyId: context.PolicyId ?? "default",
            version: "1",
            subject: context.ActorId,
            action: context.Action,
            resource: context.Domain,
            contextHash: contextHash);

        var executionHash = PolicyAnchorHashService.ComputeExecutionHash(
            decisionHash, _clock.UtcNowOffset);

        // 5. Resolve anchoring mode from policy decision
        var anchoringMode = policyResult.RequiresAnchoring
            ? (policyResult.IsStrictAnchoring ? ChainAnchoringMode.Strict : ChainAnchoringMode.Async)
            : ChainAnchoringMode.None;

        var policyDecision = new DomainPolicyDecision
        {
            DecisionType = policyResult.DecisionType,
            IsCompliant = policyResult.IsCompliant,
            RequiresAnchoring = policyResult.RequiresAnchoring,
            AnchoringMode = anchoringMode,
            Violations = policyResult.Violations,
            DecisionHash = policyResult.DecisionHash,
            PolicyIds = policyResult.PolicyIds
        };

        // 6. Anchor to chain — behavior controlled by policy anchoring mode
        var chainAnchored = false;
        if (anchoringMode != ChainAnchoringMode.None)
        {
            try
            {
                var anchorResult = await _chainAnchor.AnchorAsync(new PolicyAnchorRequest
                {
                    PolicyId = context.PolicyId ?? "default",
                    Version = "1",
                    Decision = policyResult.DecisionType,
                    Subject = context.ActorId,
                    Resource = context.Domain,
                    Action = context.Action,
                    ContextHash = contextHash,
                    EvaluationHash = policyResult.DecisionHash ?? decisionHash,
                    DecisionHash = decisionHash,
                    Timestamp = context.Timestamp,
                    SubjectId = context.ActorId
                }, ct);

                chainAnchored = anchorResult.Success || anchorResult.AlreadyAnchored;
                _metrics.RecordChainAnchor(chainAnchored);
            }
            catch (Exception ex)
            {
                _metrics.RecordChainAnchor(false);
                _anomalyEmitter.Emit(new EnforcementAnomalySignal
                {
                    Type = "CHAIN_ANCHOR_FAILURE",
                    CorrelationId = context.CorrelationId,
                    Description = $"Chain anchoring failed: {ex.Message}",
                    CommandType = context.CommandType ?? context.Action,
                    Timestamp = _clock.UtcNowOffset
                });

                // STRICT mode: anchor failure = operation failure
                if (anchoringMode == ChainAnchoringMode.Strict)
                {
                    throw new ChainAnchorFailureException(
                        decisionHash, context.CorrelationId,
                        $"Critical chain anchoring failed for {context.Action} on {context.Domain}: {ex.Message}");
                }
            }
        }

        // 7. Record latency and outcome
        var duration = _clock.UtcNowOffset - startTime;
        _metrics.RecordEnforcementLatency("domain_service", duration);
        _metrics.RecordEnforcementOutcome("executed", context.CommandType ?? context.Action, 0);

        return new DomainResult<T>
        {
            Success = true,
            Data = data,
            DecisionHash = decisionHash,
            ExecutionHash = executionHash,
            ChainAnchored = chainAnchored,
            Decision = policyDecision
        };
    }

    /// <summary>
    /// Backward-compatible wrapper returning DomainOperationResult.
    /// </summary>
    protected Task<DomainOperationResult> ExecuteGovernedUntypedAsync(
        DomainExecutionContext context,
        Func<Task<(Guid? aggregateId, object? data)>> operation,
        CancellationToken ct = default)
        => ExecuteGovernedUntypedAsync(context, operation, domainPolicyContext: null, ct);

    protected async Task<DomainOperationResult> ExecuteGovernedUntypedAsync(
        DomainExecutionContext context,
        Func<Task<(Guid? aggregateId, object? data)>> operation,
        object? domainPolicyContext,
        CancellationToken ct = default)
    {
        var result = await ExecuteGovernedAsync(context, async () =>
        {
            var (aggregateId, data) = await operation();
            return new UntypedPayload(aggregateId, data);
        }, domainPolicyContext, ct);

        if (!result.Success)
            return DomainOperationResult.Fail(
                result.Error?.Message ?? "Unknown error",
                result.Error?.Code);

        return DomainOperationResult.Ok(
            result.Data?.AggregateId,
            result.Data?.Data);
    }

    private sealed record UntypedPayload(Guid? AggregateId, object? Data);
}
