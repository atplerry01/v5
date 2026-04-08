namespace Whyce.Shared.Contracts.Infrastructure.Admission;

/// <summary>
/// Tunable behavior for the runtime HTTP intake admission limiter.
///
/// phase1.5-S5.2.1 / PC-1 (INTAKE-CONFIG-01): closes R-01 by introducing
/// a declared, configuration-bound concurrency ceiling at the Kestrel
/// edge. Follows the phase1.6-S1.5 OutboxOptions / phase1.5-S5.2.1
/// OpaOptions precedent — a plain record constructed at the composition
/// root from <c>IConfiguration</c>, no <c>IOptions&lt;T&gt;</c>
/// indirection. Defaults are conservative: an unconfigured deployment
/// gains a real but generous ceiling rather than the previous
/// unbounded shape.
///
/// Refusal semantics are non-allowing by construction: every field on
/// this record affects how an excess request is *refused*, never
/// whether it bypasses the limiter. The canonical refusal class is
/// RETRYABLE REFUSAL — HTTP 429 with a <c>Retry-After</c> header.
/// </summary>
public sealed record IntakeOptions
{
    /// <summary>
    /// Maximum number of concurrently in-flight requests admitted by
    /// the runtime intake limiter, summed across all partitions. Acts
    /// as the per-partition concurrency ceiling under the partitioned
    /// concurrency limiter shape. Must be at least 1. Default 256.
    /// </summary>
    public int GlobalConcurrency { get; init; } = 256;

    /// <summary>
    /// Maximum number of requests that may queue waiting for an
    /// admission slot per partition. Set to 0 to refuse immediately
    /// when the concurrency ceiling is reached (REJECT shape). Default
    /// 64 (RETRYABLE REFUSAL with brief queueing).
    /// </summary>
    public int QueueLimit { get; init; } = 64;

    /// <summary>
    /// Per-tenant concurrency ceiling, applied when a tenant
    /// identifier is present on the request (currently the
    /// <c>X-Tenant-Id</c> header). When absent, requests are
    /// partitioned by remote IP and this value is ignored. Must be at
    /// least 1. Default 32.
    /// </summary>
    public int PerTenantConcurrency { get; init; } = 32;

    /// <summary>
    /// Declared response class on overflow. Locked to
    /// <c>RetryableRefusal</c> in Phase 1.5: §5.2.1 forbids any shape
    /// that could collapse to an implicit allow, and the canonical
    /// 4-way model only admits one refusal class for transient intake
    /// saturation. The field exists so the canonical configuration
    /// block records the choice explicitly per the
    /// no-incidental-defaults rule (R-10).
    /// </summary>
    public string RejectionResponse { get; init; } = "RetryableRefusal";

    /// <summary>
    /// <c>Retry-After</c> header value, in seconds, returned with the
    /// 429 response. Conservative default keeps clients backing off
    /// long enough that a transient burst clears. Default 1 second.
    /// </summary>
    public int RetryAfterSeconds { get; init; } = 1;
}
