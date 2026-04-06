using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Chain;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Policy;
using Whycespace.Runtime.Context.Economic;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.Context;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Retry.PolicyAnchor;
using Whycespace.Runtime.Workflow;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Contracts.Chain;
using Whycespace.Shared.Contracts.Identity;
using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Errors;
using Whycespace.Shared.Utils;
using PolicyDecision = Whycespace.Runtime.ControlPlane.Policy.PolicyDecision;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public sealed class PolicyMiddleware : IMiddleware
{
    private readonly IPolicyEngineInvoker _engineInvoker;
    private readonly PolicyExecutionMode _mode;
    private readonly IEventPublisher? _eventPublisher;
    private readonly IPolicyDecisionAnchor? _anchor;
    private readonly PolicyAnchorRetryQueue? _retryQueue;
    private readonly MetricsCollector? _metrics;
    private readonly EnforcementAnomalyEmitter? _anomalyEmitter;
    private readonly ILogger<PolicyMiddleware>? _logger;
    private readonly IdentityContextResolver _identityResolver = new();
    private readonly EconomicContextResolver _economicResolver = new();
    private readonly WorkflowContextResolver _workflowResolver = new();

    public PolicyMiddleware(
        IPolicyEngineInvoker engineInvoker,
        PolicyExecutionMode mode = PolicyExecutionMode.Enforcement,
        IEventPublisher? eventPublisher = null,
        IPolicyDecisionAnchor? anchor = null,
        PolicyAnchorRetryQueue? retryQueue = null,
        MetricsCollector? metrics = null,
        EnforcementAnomalyEmitter? anomalyEmitter = null,
        ILogger<PolicyMiddleware>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(engineInvoker);
        _engineInvoker = engineInvoker;
        _mode = mode;
        _eventPublisher = eventPublisher;
        _anchor = anchor;
        _retryQueue = retryQueue;
        _metrics = metrics;
        _anomalyEmitter = anomalyEmitter;
        _logger = logger;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // STEP 1 — Map context via PolicyContextMapper
        var input = PolicyContextMapper.Map(context);

        // STEP 2 — Invoke via engine invoker with mode
        var result = await _engineInvoker.InvokeAsync(input, _mode, context.CancellationToken);

        // STEP 3 — Attach enforcement context (always, for both modes)
        var enforcement = PolicyEnforcementContext.FromResult(result);
        context.Set(ContextKeys.EnforcementContext, enforcement);

        if (result.EventPayload is not null)
            context.Set(ContextKeys.PolicyEventPayload, result.EventPayload);

        // STEP 3b — Attach PolicyDecision contract (PEL — Policy Enforcement Lock)
        var policyIds = ExtractPolicyIds(result);
        var evalHash = PolicyDecision.ComputeEvaluationHash(
            context.Envelope.CommandId, context.Envelope.CommandType, context.Envelope.CorrelationId);

        PolicyDecision decision = result.IsDenied
            ? PolicyDecision.Deny(policyIds, evalHash, result.Violation ?? "Policy denied.", context.Clock)
            : result.IsConditional
                ? PolicyDecision.Conditional(policyIds, evalHash, result.Violations, context.Clock)
                : PolicyDecision.Allow(policyIds, evalHash, context.Clock);

        context.Set(PolicyDecision.ContextKey, decision);

        // STEP 3c — Policy drift detection (non-blocking)
        if (result.IsConditional && _anomalyEmitter is not null)
        {
            _anomalyEmitter.Emit(new EnforcementAnomalySignal
            {
                Type = EnforcementAnomalyEmitter.AnomalyTypes.PolicyDrift,
                CorrelationId = context.Envelope.CorrelationId,
                CommandType = context.Envelope.CommandType,
                Description = "Conditional policy decision detected — possible policy drift"
            });
        }

        // STEP 4 — Publish policy events (Kafka via outbox)
        if (_eventPublisher is not null)
        {
            var policyEvent = new RuntimeEvent
            {
                EventId = DeterministicIdHelper.FromSeed($"policy:evaluated:{context.Envelope.CommandId}:{context.Envelope.CorrelationId}"),
                AggregateId = Guid.Empty,
                EventType = "PolicyEvaluatedEvent",
                AggregateType = "PolicyAudit",
                CorrelationId = context.Envelope.CorrelationId,
                CommandId = context.Envelope.CommandId,
                ExecutionId = context.Envelope.CommandType,
                Payload = new
                {
                    decision = result.IsDenied ? "DENY" : result.IsConditional ? "CONDITIONAL" : "ALLOW",
                    violation = result.Violation,
                    policyIds = ExtractPolicyIds(result)
                },
                Timestamp = context.Clock.UtcNowOffset
            };
            await _eventPublisher.PublishAsync(policyEvent, context.CancellationToken);
        }

        // STEP 4b — Anchor decision to WhyceChain (E4 — non-blocking, tracked)
        // Task is stored on context for observability. AnchorDecisionSafeAsync
        // has its own try/catch — failure never propagates to the pipeline.
        if (_anchor is not null)
        {
            var anchorTask = AnchorDecisionSafeAsync(decision, input, result, context);
            context.Set(ContextKeys.AnchorTask, anchorTask);
        }

        // STEP 5 — In ENFORCEMENT mode, block on DENY
        if (_mode == PolicyExecutionMode.Enforcement && result.IsDenied)
        {
            var error = new PolicyDeniedError
            {
                PolicyIds = ExtractPolicyIds(result),
                ViolatedRules = ExtractViolatedRules(result),
                Reason = result.Violation ?? "Policy evaluation denied.",
                CorrelationId = context.Envelope.CorrelationId
            };

            return CommandResult.Fail(
                context.Envelope.CommandId,
                error.ToErrorMessage(),
                PolicyDeniedError.ErrorCode,
                context.Clock.UtcNowOffset);
        }

        // SIMULATION mode: attach result but NEVER block — always continue
        return await next(context);
    }

    private static IReadOnlyList<Guid> ExtractPolicyIds(PolicyEvaluationResult result)
    {
        return result.EventPayload is not null && result.EventPayload.PolicyId != Guid.Empty
            ? [result.EventPayload.PolicyId]
            : [];
    }

    private static IReadOnlyList<string> ExtractViolatedRules(PolicyEvaluationResult result)
    {
        return result.EvaluatedRules
            .Where(r => !r.Passed)
            .Select(r => r.Reason ?? $"Rule {r.RuleId}")
            .ToList();
    }

    /// <summary>
    /// Anchors the policy decision to WhyceChain. Non-blocking — failure does NOT fail the command.
    /// </summary>
    private async Task AnchorDecisionSafeAsync(
        PolicyDecision decision,
        PolicyEvaluationInput input,
        PolicyEvaluationResult result,
        CommandContext context)
    {
        var policyId = input.PolicyId?.ToString() ?? "unknown";
        var subject = input.ActorId.ToString();
        var contextHash = string.Empty;
        var decisionHash = string.Empty;
        IIdentityContext? identity = null;

        try
        {
            // E5: Resolve identity context
            identity = _identityResolver.Resolve(context);

            // E6: Resolve economic context (null for non-economic commands)
            var economic = _economicResolver.Resolve(context);

            // E7: Resolve workflow context (null for non-workflow commands)
            var workflow = _workflowResolver.Resolve(context);

            contextHash = PolicyAnchorHashService.ComputeContextHash(
                subject, input.Resource, input.Action);

            // E4.1 + E5 + E6 + E7: DecisionHash includes identity + economic + workflow fields
            decisionHash = PolicyAnchorHashService.ComputeDecisionHash(
                policyId, "1", subject, input.Action, input.Resource, contextHash,
                identity.SubjectId, identity.Roles, identity.TrustScore, identity.IsVerified,
                economic?.AccountId, economic?.AssetId, economic?.Amount, economic?.Currency,
                economic?.TransactionType,
                workflow?.WorkflowId, workflow?.StepId, workflow?.State, workflow?.Transition);

            // E6: Link decision hash to economic execution context
            if (economic is not null)
                EconomicChainLinker.LinkDecision(context, decisionHash);

            // E7: Link decision hash to workflow execution context
            if (workflow is not null)
                WorkflowChainLinker.LinkDecision(context, decisionHash,
                    workflow.WorkflowId, workflow.StepId, workflow.State, workflow.Transition);

            var anchorRequest = new PolicyAnchorRequest
            {
                PolicyId = policyId,
                Version = "1",
                Decision = decision.Result.ToString().ToUpperInvariant(),
                Subject = subject,
                Resource = input.Resource,
                Action = input.Action,
                ContextHash = contextHash,
                EvaluationHash = decision.EvaluationHash,
                DecisionHash = decisionHash,
                Timestamp = decision.Timestamp,
                // E5: Identity binding
                SubjectId = identity.SubjectId,
                Roles = identity.Roles,
                TrustScore = identity.TrustScore,
                IsVerified = identity.IsVerified,
                SessionId = identity.SessionId,
                DeviceId = identity.DeviceId,
                // E6: Economic binding
                AccountId = economic?.AccountId,
                AssetId = economic?.AssetId,
                Amount = economic?.Amount,
                Currency = economic?.Currency,
                TransactionType = economic?.TransactionType,
                // E7: Workflow binding
                WorkflowId = workflow?.WorkflowId,
                StepId = workflow?.StepId,
                WorkflowState = workflow?.State,
                Transition = workflow?.Transition
            };

            await _anchor!.AnchorAsync(anchorRequest, context.CancellationToken);
        }
        catch (Exception ex)
        {
            // Non-blocking: anchor failure must never fail the main request.
            // Enqueue for retry and track failure metrics.
            _metrics?.Increment(MetricNames.PolicyAnchorFailed);
            _logger?.LogError(ex,
                "Policy anchor failed for decision={DecisionId} — queued for retry",
                decision.DecisionId);

            if (_retryQueue is not null)
            {
                _retryQueue.Enqueue(new PolicyAnchorRequest
                {
                    PolicyId = policyId,
                    Version = "1",
                    Decision = decision.Result.ToString().ToUpperInvariant(),
                    Subject = subject,
                    Resource = input.Resource,
                    Action = input.Action,
                    ContextHash = contextHash,
                    EvaluationHash = decision.EvaluationHash,
                    DecisionHash = decisionHash,
                    Timestamp = decision.Timestamp,
                    SubjectId = identity?.SubjectId ?? "",
                    Roles = identity?.Roles ?? [],
                    TrustScore = identity?.TrustScore ?? 0.0,
                    IsVerified = identity?.IsVerified ?? false
                });
            }
        }
    }

    public static class ContextKeys
    {
        public const string EnforcementContext = "Policy.EnforcementContext";
        public const string PolicyEventPayload = "Policy.EventPayload";
        public const string AnchorTask = "Policy.AnchorTask";
    }

    public static class MetricNames
    {
        public const string PolicyAnchorFailed = "runtime.policy.anchor.failed";
        public const string PolicyAnchorFailureRateExceeded = "runtime.policy.anchor.failure_rate_exceeded";
    }
}
