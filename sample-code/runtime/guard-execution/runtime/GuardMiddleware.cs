using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.ControlPlane.Policy;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Sharding;
using Whycespace.Runtime.WhyceChain;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Dual-phase runtime middleware that evaluates guards at a specific pipeline phase.
/// PrePolicy instance runs before PolicyMiddleware.
/// PostPolicy instance runs after PolicyMiddleware.
///
/// PostPolicy phase additionally:
/// - Builds EnforcementContext binding guard + policy + chain
/// - Computes deterministic hashes for chain anchoring
/// - Records enforcement decisions to WhyceChain
/// - Escalates violations and publishes policy feedback
/// </summary>
public sealed class GuardMiddleware : IMiddleware
{
    private readonly GuardPhase _phase;
    private readonly IGuardExecutionEngine _engine;
    private readonly ILogger<GuardMiddleware> _logger;
    private readonly EnforcementChainAnchor? _chainAnchor;
    private readonly ViolationEscalationEngine? _escalation;
    private readonly PolicyFeedbackPublisher? _feedbackPublisher;
    private readonly EnforcementMetrics? _enforcementMetrics;
    private readonly IClock _clock;
    private readonly int _partitionCount;
    private readonly int _shardCount;

    public const string PrePolicyReportKey = "Guard.PrePolicy.Report";
    public const string PostPolicyReportKey = "Guard.PostPolicy.Report";
    public const string GuardValidatedKey = "Guard.IsValidated";

    public const int DefaultPartitionCount = 16;
    public const int DefaultShardCount = 4;

    public GuardMiddleware(
        GuardPhase phase,
        IGuardExecutionEngine engine,
        ILogger<GuardMiddleware> logger,
        IClock clock,
        EnforcementChainAnchor? chainAnchor = null,
        ViolationEscalationEngine? escalation = null,
        PolicyFeedbackPublisher? feedbackPublisher = null,
        EnforcementMetrics? enforcementMetrics = null,
        int partitionCount = DefaultPartitionCount,
        int shardCount = DefaultShardCount)
    {
        _phase = phase;
        _engine = engine;
        _logger = logger;
        _clock = clock;
        _chainAnchor = chainAnchor;
        _escalation = escalation;
        _feedbackPublisher = feedbackPublisher;
        _enforcementMetrics = enforcementMetrics;
        _partitionCount = partitionCount > 0 ? partitionCount : DefaultPartitionCount;
        _shardCount = shardCount > 0 ? shardCount : DefaultShardCount;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Replay bypass: in Replay mode, guards are NOT re-evaluated.
        // Stored hashes from the original execution are reused.
        var storedEnforcement = context.Get<EnforcementContext>(EnforcementContext.ContextKey);
        if (storedEnforcement is { ExecutionMode: ExecutionMode.Replay })
        {
            return await next(context);
        }

        var guardContext = GuardContext.ForRuntime(context);

        var guardStart = Stopwatch.GetTimestamp();
        var report = await _engine.ExecutePhaseAsync(guardContext, _phase, context.CancellationToken);
        _enforcementMetrics?.RecordEnforcementLatency(
            $"guard.{_phase}", Stopwatch.GetElapsedTime(guardStart));

        // Record per-guard metrics (non-blocking)
        foreach (var result in report.Results)
        {
            var severity = result.Violations.FirstOrDefault()?.Severity.ToString();
            _enforcementMetrics?.RecordGuardExecution(
                result.GuardName, _phase.ToString(), result.Passed, severity);
        }

        // Attach phase-specific report to context
        var reportKey = _phase == GuardPhase.PrePolicy ? PrePolicyReportKey : PostPolicyReportKey;
        context.Set(reportKey, report);

        // Escalate violations regardless of outcome
        if (_escalation is not null && report.AllViolations.Count > 0)
        {
            await _escalation.EscalateAsync(report, context.Envelope.CorrelationId, context.CancellationToken);
        }

        // Check for blocking violations
        if (report.Status == GuardExecutionStatus.Fail)
        {
            var firstViolation = report.BlockingViolations.FirstOrDefault();

            _logger.LogWarning(
                "Guard {Phase} BLOCKED command {CommandType}: {Rule} — {Description}",
                _phase,
                context.Envelope.CommandType,
                firstViolation?.Rule ?? "UNKNOWN",
                firstViolation?.Description ?? "Guard violation detected");

            // On PostPolicy failure: build enforcement context, anchor, and publish feedback
            if (_phase == GuardPhase.PostPolicy)
            {
                await HandlePostPolicyEnforcement(context, report, EnforcementOutcome.BlockedByGuard);
            }

            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Guard violation ({_phase}): {firstViolation?.Description ?? "Execution blocked by guard"}",
                firstViolation?.Rule ?? "GUARD.BLOCKED");
        }

        if (report.Status == GuardExecutionStatus.Warn)
        {
            _logger.LogWarning(
                "Guard {Phase} warnings for command {CommandType}: {Count} warnings",
                _phase,
                context.Envelope.CommandType,
                report.Warnings.Count);
        }

        // PostPolicy phase: build enforcement context, mark validated, anchor on success
        if (_phase == GuardPhase.PostPolicy)
        {
            context.Set(GuardValidatedKey, true);

            var enforcement = await HandlePostPolicyEnforcement(context, report, EnforcementOutcome.Executed);
            context.Set(EnforcementContext.ContextKey, enforcement);

            // Record guard decision to WhyceChain audit trail
            RecordGuardDecision(context, enforcement);
        }

        return await next(context);
    }

    private async Task<EnforcementContext> HandlePostPolicyEnforcement(
        CommandContext context,
        GuardExecutionReport postReport,
        EnforcementOutcome outcome)
    {
        var preReport = context.Get<GuardExecutionReport>(PrePolicyReportKey);
        var policyDecision = context.Get<PolicyDecision>(PolicyDecision.ContextKey);

        // Resolve deterministic partition from CommandContext (set by RuntimeControlPlane)
        var partitionKey = context.PartitionKey ?? context.Envelope.CorrelationId;
        var partitionId = DeterministicPartitionResolver.ResolvePartition(partitionKey, _partitionCount);
        var shardId = DeterministicPartitionResolver.ResolveShardId(partitionKey, _shardCount);

        var enforcement = new EnforcementContext
        {
            CommandName = context.Envelope.CommandType,
            Payload = context.Envelope.Payload,
            CorrelationId = context.Envelope.CorrelationId,
            CausationId = context.Envelope.Metadata.CausationId,
            PrePolicyReport = preReport,
            PostPolicyReport = postReport,
            PolicyDecision = policyDecision,
            DecisionHash = policyDecision is not null
                ? PolicyDecisionHasher.ComputeDecisionHash(policyDecision)
                : null,
            GuardHash = GuardResultHasher.ComputeCombinedHash(preReport, postReport),
            Timestamp = _clock.UtcNowOffset,
            Outcome = outcome,
            PartitionKey = partitionKey,
            PartitionId = partitionId,
            ShardId = shardId
        };

        // Anchor to WhyceChain (both success and failure) — MANDATORY, failure blocks execution
        if (_chainAnchor is not null)
        {
            try
            {
                await _chainAnchor.AnchorAsync(enforcement, context.CancellationToken);
            }
            catch (EnforcementChainException)
            {
                enforcement.Outcome = EnforcementOutcome.BlockedByChain;
                throw;
            }
        }

        // Record enforcement outcome metric (non-blocking)
        _enforcementMetrics?.RecordEnforcementOutcome(
            enforcement.Outcome.ToString(), enforcement.CommandName, enforcement.PartitionId);

        // Publish feedback on violations
        if (_feedbackPublisher is not null && postReport.AllViolations.Count > 0)
        {
            await _feedbackPublisher.PublishFeedbackAsync(
                postReport, context.Envelope.CorrelationId,
                context.Envelope.CommandType, context.CancellationToken);
        }

        return enforcement;
    }

    private static void RecordGuardDecision(CommandContext context, EnforcementContext enforcement)
    {
        var decision = new DecisionEnvelope
        {
            DecisionId = DeterministicIdHelper.FromSeed(
                $"guard-decision:{context.Envelope.CorrelationId}:{context.Envelope.CommandType}:{enforcement.Outcome}"),
            ExecutionId = context.ExecutionId,
            CommandId = context.Envelope.CommandId,
            CorrelationId = context.Envelope.CorrelationId,
            DecisionType = DecisionTypes.ExecutionGuard,
            Outcome = enforcement.Outcome == EnforcementOutcome.Executed
                ? DecisionOutcomes.Compliant
                : DecisionOutcomes.NonCompliant,
            Timestamp = enforcement.Timestamp,
            Reason = enforcement.Outcome == EnforcementOutcome.Executed
                ? null
                : $"Blocked: {enforcement.Outcome}",
            Context = new Dictionary<string, string>
            {
                ["guardHash"] = enforcement.GuardHash ?? "none",
                ["decisionHash"] = enforcement.DecisionHash ?? "none",
                ["outcome"] = enforcement.Outcome.ToString()
            }
        };

        DecisionRecordingMiddleware.RecordDecision(context, decision);
    }
}
