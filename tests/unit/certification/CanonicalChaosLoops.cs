namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.C.2 Phase 1 — C# mirror of
/// <c>infrastructure/observability/certification/chaos-observability-loop.yml</c>.
/// Each entry binds an R5.B failure mode to its full observability signal
/// chain. Validators cross-reference against R5.B, R4.A, and R5.A catalogs.
/// </summary>
public static class CanonicalChaosLoops
{
    public sealed record ChaosLoop(
        string Id,
        string FailureMode,
        string? Exception,
        string? Handler,
        int? HttpStatus,
        string? TypeUri,
        string? FeedingMetric,
        string? R4aAlert,
        string SpanFamily,
        string SpanExpectedStatus,
        string LoopProofStatus,
        string? ProofTest = null);

    public const string CatalogedStatus = "cataloged";
    public const string LiveProvenStatus = "live_proven";

    public static readonly IReadOnlyList<ChaosLoop> All = new ChaosLoop[]
    {
        new(
            Id: "opa-unavailable-loop",
            FailureMode: "opa-unavailable",
            Exception: "PolicyEvaluationUnavailableException",
            Handler: "PolicyEvaluationUnavailableExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:policy-evaluation-unavailable",
            FeedingMetric: "policy_evaluate_breaker_open_total",
            R4aAlert: "RuntimePolicyBreakerOpenRate",
            SpanFamily: "runtime.command.dispatch",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "outbox-saturated-loop",
            FailureMode: "outbox-saturated",
            Exception: "OutboxSaturatedException",
            Handler: "OutboxSaturatedExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:outbox-saturated",
            FeedingMetric: "outbox_depth",
            R4aAlert: "OutboxDepthHigh",
            SpanFamily: "event.fabric.process",
            SpanExpectedStatus: "Error",
            LoopProofStatus: LiveProvenStatus,
            ProofTest: "tests/integration/chaos/OutboxSaturatedChaosLoopTest.cs"),

        new(
            Id: "chain-anchor-wait-timeout-loop",
            FailureMode: "chain-anchor-wait-timeout",
            Exception: "ChainAnchorWaitTimeoutException",
            Handler: "ChainAnchorWaitTimeoutExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:chain-anchor-wait-timeout",
            FeedingMetric: "chain_anchor_wait_ms_bucket",
            R4aAlert: "ChainAnchorWaitP95High",
            SpanFamily: "event.fabric.process",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "chain-anchor-unavailable-loop",
            FailureMode: "chain-anchor-unavailable",
            Exception: "ChainAnchorUnavailableException",
            Handler: "ChainAnchorUnavailableExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:chain-anchor-unavailable",
            FeedingMetric: null,
            R4aAlert: null,
            SpanFamily: "event.fabric.process",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "workflow-saturated-loop",
            FailureMode: "workflow-saturated",
            Exception: "WorkflowSaturatedException",
            Handler: "WorkflowSaturatedExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:workflow-saturated",
            FeedingMetric: "workflow_rejected_total",
            R4aAlert: "WorkflowAdmissionRejectionSustained",
            SpanFamily: "runtime.command.dispatch",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "workflow-timeout-loop",
            FailureMode: "workflow-timeout",
            Exception: "WorkflowTimeoutException",
            Handler: "WorkflowTimeoutExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:workflow-timeout",
            FeedingMetric: "workflow_execution_duration_count",
            R4aAlert: "WorkflowTimeoutRateHigh",
            SpanFamily: "runtime.command.dispatch",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "postgres-pool-exhaustion-loop",
            FailureMode: "postgres-pool-exhaustion",
            Exception: null,
            Handler: null,
            HttpStatus: null,
            TypeUri: null,
            FeedingMetric: "postgres_pool_acquisition_failures_total",
            R4aAlert: "PostgresPoolAcquisitionFailuresHigh",
            SpanFamily: "event.fabric.process",
            SpanExpectedStatus: "Error",
            LoopProofStatus: CatalogedStatus),

        new(
            Id: "concurrency-conflict-loop",
            FailureMode: "concurrency-conflict",
            Exception: "ConcurrencyConflictException",
            Handler: "ConcurrencyConflictExceptionHandler",
            HttpStatus: 409,
            TypeUri: "urn:whyce:error:concurrency-conflict",
            FeedingMetric: "event_store_append_advisory_lock_wait_ms_bucket",
            R4aAlert: "EventStoreAdvisoryWaitP95High",
            SpanFamily: "event.fabric.process",
            SpanExpectedStatus: "Error",
            LoopProofStatus: LiveProvenStatus,
            ProofTest: "tests/integration/chaos/ConcurrencyConflictChaosLoopTest.cs"),

        new(
            Id: "domain-invariant-violation-loop",
            FailureMode: "domain-invariant-violation",
            Exception: "DomainException",
            Handler: "DomainExceptionHandler",
            HttpStatus: 400,
            TypeUri: "urn:whyce:error:domain-invariant",
            FeedingMetric: null,
            R4aAlert: null,
            SpanFamily: "runtime.command.dispatch",
            SpanExpectedStatus: "Error",
            LoopProofStatus: LiveProvenStatus,
            ProofTest: "tests/integration/chaos/DomainInvariantChaosLoopTest.cs"),
    };

    /// <summary>
    /// Canonical log-scope keys every loop inherits per
    /// R-TRACE-LOG-CORRELATION-01. Required keys MUST be present; optional
    /// keys MAY be present.
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredLogScopeKeys =
        new[] { "trace_id", "span_id", "correlation_id" };

    public static readonly IReadOnlyList<string> OptionalLogScopeKeys =
        new[] { "tenant_id" };

    /// <summary>
    /// Canonical span-name set — mirrors
    /// <c>Whycespace.Runtime.Observability.WhyceActivitySources.Spans.*</c>
    /// constants. Kept here as a string list so tests can cross-reference
    /// without a runtime-layer dependency cycle.
    /// </summary>
    public static readonly IReadOnlyList<string> CanonicalSpanFamilies = new[]
    {
        "runtime.command.dispatch",
        "runtime.admin.operator_action",
        "event.fabric.process",
        "event.fabric.process_audit",
        "outbound.effect.schedule",
        "outbound.effect.dispatch",
        "outbound.effect.finalize",
        "outbound.effect.reconcile",
    };
}
