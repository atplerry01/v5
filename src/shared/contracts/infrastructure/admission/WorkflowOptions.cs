namespace Whyce.Shared.Contracts.Infrastructure.Admission;

/// <summary>
/// Tunable behavior for the runtime workflow in-flight admission gate.
///
/// phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): closes K-R-05 by
/// introducing a declared per-workflow-name and per-tenant in-flight
/// ceiling on <c>WorkflowStartCommand</c> / <c>WorkflowResumeCommand</c>
/// execution. Pre-KC-6 the dispatcher had no concurrency primitive
/// at the workflow seam — bursts of identical workflow starts ran
/// concurrently up to whatever the upstream <c>IntakeOptions</c>
/// admission ceiling permitted, with no per-name fairness and no
/// per-tenant cap.
///
/// Follows the phase1.6-S1.5 OutboxOptions / phase1.5-S5.2.1
/// OpaOptions / IntakeOptions / phase1.5-S5.2.2 KafkaConsumerOptions
/// precedent — a plain record bound at the composition root from
/// configuration, no <c>IOptions&lt;T&gt;</c> indirection. Defaults
/// are conservative and sized below the §5.2.2 KC-1 intake envelope
/// so the gate refuses *before* the intake limiter and the pool
/// ever come close to saturation.
///
/// Refusal semantics are non-allowing: every field affects how an
/// excess workflow start is *refused*, never whether it bypasses the
/// gate. The canonical refusal class is RETRYABLE REFUSAL — typed
/// <c>WorkflowSaturatedException</c> mapped to HTTP 503 +
/// <c>Retry-After</c> at the API edge, mirroring the PC-2 / PC-3
/// canonical edge handlers from §5.2.1.
/// </summary>
public sealed record WorkflowOptions
{
    /// <summary>
    /// Maximum number of concurrently in-flight executions for any
    /// single workflow name. Acts as the per-partition concurrency
    /// ceiling for the partitioned admission gate keyed by workflow
    /// name. Must be at least 1. Default 4 — sized below the §5.2.2
    /// KC-1 per-tenant intake envelope (4) so a single saturating
    /// workflow cannot fully consume per-tenant intake headroom.
    /// </summary>
    public int PerWorkflowConcurrency { get; init; } = 4;

    /// <summary>
    /// Maximum number of concurrently in-flight workflow executions
    /// for any single tenant, summed across workflow names. Applied
    /// when a tenant identifier is present on the command context
    /// (currently <c>"default"</c> for every command — the partition
    /// is wired so real tenants acquire their own slots when
    /// tenancy lands). Must be at least 1. Default 6 — strict
    /// upper bound on tenant-side workflow concurrency below the
    /// KC-1 per-IP intake ceiling (6).
    /// </summary>
    public int PerTenantConcurrency { get; init; } = 6;

    /// <summary>
    /// Maximum number of workflow starts that may queue waiting for
    /// an admission slot per partition. Set to 0 to refuse
    /// immediately when the concurrency ceiling is reached (REJECT
    /// shape). Default 8 — brief queueing absorbs short bursts
    /// without stretching the workflow latency tail.
    /// </summary>
    public int QueueLimit { get; init; } = 8;

    /// <summary>
    /// <c>Retry-After</c> header value, in seconds, returned with the
    /// 503 response. Sized to give the in-flight workflows time to
    /// complete and free a slot. Default 5 seconds.
    /// </summary>
    public int RetryAfterSeconds { get; init; } = 5;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): per-step timeout
    /// in milliseconds. Each <see cref="IWorkflowStep.ExecuteAsync"/>
    /// call runs under a per-step linked CTS bounded by this value;
    /// on expiry the engine throws the typed
    /// <c>WorkflowTimeoutException("step", stepName, …)</c> RETRYABLE
    /// REFUSAL. Mirrors the OpaOptions.RequestTimeoutMs precedent —
    /// a single declared per-call ceiling, no implicit defaults from
    /// the underlying primitive. Must be at least 1. Default 30000 ms.
    /// </summary>
    public int PerStepTimeoutMs { get; init; } = 30_000;

    /// <summary>
    /// phase1.5-S5.2.3 / TC-7 (WORKFLOW-TIMEOUT-01): overall workflow
    /// execution timeout in milliseconds. The engine creates an
    /// execution-level CTS linked to the upstream request/host token
    /// bounded by this value; on expiry the engine throws the typed
    /// <c>WorkflowTimeoutException("execution", null, …)</c> RETRYABLE
    /// REFUSAL. Bounds the case where every individual step finishes
    /// under its per-step ceiling but the cumulative execution still
    /// exceeds operational expectations. Must be at least 1. Default
    /// 300000 ms (5 minutes).
    /// </summary>
    public int MaxExecutionMs { get; init; } = 300_000;
}
