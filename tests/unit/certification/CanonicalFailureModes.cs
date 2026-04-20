namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.B — C# mirror of
/// <c>infrastructure/observability/certification/runtime-failure-modes.yml</c>.
/// Validators consume this record list so they do not depend on a YAML
/// parser NuGet. The YAML remains the human-readable source of truth;
/// <see cref="FailureModeManifestTests"/> verifies the two are in sync on
/// every test run.
/// </summary>
public static class CanonicalFailureModes
{
    public sealed record FailureMode(
        string Id,
        string Fault,
        string? CanonicalException,
        string? ExceptionHandler,
        int? HttpStatus,
        string? TypeUri,
        string? DegradedReason,
        IReadOnlyList<string> FeedingMetrics,
        IReadOnlyList<string> R4aAlerts,
        string? ProofTest,
        string Status);

    public sealed record OperationalOnlyAlert(string Alert, string Rationale);

    public const string CertifiedStatus = "certified";
    public const string UnprovenStatus = "unproven";

    public static readonly IReadOnlyList<FailureMode> All = new FailureMode[]
    {
        new(
            Id: "opa-unavailable",
            Fault: "OPA policy evaluator transport failure or breaker-open",
            CanonicalException: "PolicyEvaluationUnavailableException",
            ExceptionHandler: "PolicyEvaluationUnavailableExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:policy-evaluation-unavailable",
            DegradedReason: "opa_breaker_open",
            FeedingMetrics: new[]
            {
                "policy_evaluate_timeout_total",
                "policy_evaluate_breaker_open_total",
                "policy_evaluate_failure_total",
            },
            R4aAlerts: new[] { "RuntimePolicyEvaluationFailuresSpike", "RuntimePolicyBreakerOpenRate" },
            ProofTest: "tests/integration/failure-recovery/PolicyEngineFailureTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "outbox-saturated",
            Fault: "Outbox depth above high-water-mark admission threshold",
            CanonicalException: "OutboxSaturatedException",
            ExceptionHandler: "OutboxSaturatedExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:outbox-saturated",
            DegradedReason: "outbox_over_high_water_mark",
            FeedingMetrics: new[] { "outbox_depth", "outbox_oldest_pending_age_seconds" },
            R4aAlerts: new[] { "OutboxDepthHigh", "OutboxOldestPendingAgeHigh" },
            ProofTest: "tests/integration/failure-recovery/OutboxKafkaOutageRecoveryTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "chain-anchor-wait-timeout",
            Fault: "Chain-anchor commit serializer wait exceeds bounded timeout",
            CanonicalException: "ChainAnchorWaitTimeoutException",
            ExceptionHandler: "ChainAnchorWaitTimeoutExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:chain-anchor-wait-timeout",
            DegradedReason: "chain_anchor_breaker_open",
            FeedingMetrics: new[] { "chain_anchor_wait_ms_bucket" },
            R4aAlerts: new[] { "ChainAnchorWaitP95High" },
            ProofTest: "tests/integration/failure-recovery/ChainFailureTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "chain-anchor-unavailable",
            Fault: "Chain-store transport failure or breaker-open",
            CanonicalException: "ChainAnchorUnavailableException",
            ExceptionHandler: "ChainAnchorUnavailableExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:chain-anchor-unavailable",
            DegradedReason: "chain_anchor_breaker_open",
            FeedingMetrics: Array.Empty<string>(),
            R4aAlerts: Array.Empty<string>(),
            ProofTest: "tests/integration/failure-recovery/ChainFailureTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "workflow-saturated",
            Fault: "WorkflowAdmissionGate at concurrency capacity",
            CanonicalException: "WorkflowSaturatedException",
            ExceptionHandler: "WorkflowSaturatedExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:workflow-saturated",
            DegradedReason: null,
            FeedingMetrics: new[] { "workflow_rejected_total" },
            R4aAlerts: new[] { "WorkflowAdmissionRejectionSustained" },
            ProofTest: "tests/integration/api/WorkflowSaturatedExceptionHandlerTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "workflow-timeout",
            Fault: "Per-step or per-execution workflow deadline expiry",
            CanonicalException: "WorkflowTimeoutException",
            ExceptionHandler: "WorkflowTimeoutExceptionHandler",
            HttpStatus: 503,
            TypeUri: "urn:whyce:error:workflow-timeout",
            DegradedReason: null,
            FeedingMetrics: new[] { "workflow_execution_duration_count" },
            R4aAlerts: new[] { "WorkflowTimeoutRateHigh" },
            ProofTest: "tests/integration/api/WorkflowTimeoutExceptionHandlerTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "postgres-pool-exhaustion",
            Fault: "Postgres pool acquisition failures / high-wait",
            CanonicalException: null,
            ExceptionHandler: null,
            HttpStatus: null,
            TypeUri: null,
            DegradedReason: "postgres_high_wait",
            FeedingMetrics: new[] { "postgres_pool_acquisition_failures_total" },
            R4aAlerts: new[] { "PostgresPoolAcquisitionFailuresHigh" },
            ProofTest: "tests/integration/failure-recovery/PostgresFailureRecoveryTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "concurrency-conflict",
            Fault: "Event-store optimistic concurrency violation",
            CanonicalException: "ConcurrencyConflictException",
            ExceptionHandler: "ConcurrencyConflictExceptionHandler",
            HttpStatus: 409,
            TypeUri: "urn:whyce:error:concurrency-conflict",
            DegradedReason: null,
            FeedingMetrics: Array.Empty<string>(),
            R4aAlerts: Array.Empty<string>(),
            ProofTest: "tests/integration/api/ConcurrencyConflictExceptionHandlerTest.cs",
            Status: CertifiedStatus),

        new(
            Id: "domain-invariant-violation",
            Fault: "Aggregate refuses a command as domain-invalid",
            CanonicalException: "DomainException",
            ExceptionHandler: "DomainExceptionHandler",
            HttpStatus: 400,
            TypeUri: "urn:whyce:error:domain-invariant",
            DegradedReason: null,
            FeedingMetrics: Array.Empty<string>(),
            R4aAlerts: Array.Empty<string>(),
            ProofTest: "tests/integration/api/DomainExceptionHandlerTest.cs",
            Status: CertifiedStatus),
    };

    public static readonly IReadOnlyList<OperationalOnlyAlert> OperationalOnlyAlerts = new OperationalOnlyAlert[]
    {
        new("RuntimeNotReady", "Scrape-layer signal (up == 0) — no application exception in scope."),
        new("IntakeSustainedRejectionRate", "Admission rate-limiter refusal — load signal, not a fault."),
        new("HttpFiveHundredRateHigh", "Aggregate 5xx meta-signal — union of every 503/500 path."),
        new("WorkflowStepTerminalFailureSpike", "Workflow business-outcome signal — downstream-dependency surfacing."),
        new("WorkflowStepRetryAttemptsBurst", "Info-severity early warning for downstream instability."),
        new("OutboundEffectReconciliationBacklogGrowing", "Lifecycle-state downstream of provider slowness."),
        new("OutboundEffectCompensationUnhandled", "Governance signal — missing handler registration."),
        new("OutboundEffectRetryExhaustionSpike", "Retry-tier aggregate — provider-specific faults not cataloged."),
        new("OutboundEffectDispatchLatencyP95High", "Provider adapter envelope latency — drill-down signal."),
        new("DlqArrivalRateHigh", "Aggregate DLQ flow signal — root causes upstream."),
        new("DlqDepthSustained", "Backlog signal — pairs with DlqArrivalRateHigh."),
        new("DlqPublishFailing", "Kafka producer transport — pairs with breaker-open."),
        new("OperatorRedriveFailureSpike", "Operator-visible consequence of DlqPublishFailing."),
        new("EventStoreAdvisoryWaitP95High", "Proxy for aggregate write contention / concurrency-conflict precursor."),
    };
}
