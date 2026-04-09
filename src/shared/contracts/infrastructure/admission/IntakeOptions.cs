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
    /// Per-IP-partition in-flight ceiling under the partitioned
    /// concurrency limiter shape. Must be at least 1.
    ///
    /// phase1.5-S5.2.2 / KC-1 (CAPACITY-RESOLUTION-01): default lowered
    /// from 256 to 6 to match real event-store downstream capacity.
    /// Step B P-K3 confirmed each dispatched command performs 5
    /// sequential acquisitions on the declared event-store pool
    /// (idempotency exists + idempotency mark + load events + append
    /// events + outbox enqueue), so a single IP at this ceiling can
    /// hold at most 6 × 5 = 30 connections — within the declared
    /// Postgres.Pools.EventStore.MaxPoolSize (32). Overload from a
    /// single source therefore refuses at this limiter as HTTP 429 +
    /// Retry-After (PC-1 RETRYABLE REFUSAL) before the pool's
    /// 5-second connection-acquisition timeout fires and propagates as
    /// a generic 500. Tighten further if the per-command acquisition
    /// count grows.
    /// </summary>
    public int GlobalConcurrency { get; init; } = 6;

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
    /// least 1.
    ///
    /// phase1.5-S5.2.2 / KC-1 (CAPACITY-RESOLUTION-01): default lowered
    /// from 32 to 4 so a single tenant cannot fully consume the
    /// event-store pool (4 × 5 = 20 connections) and other tenants
    /// retain headroom. Companion to GlobalConcurrency.
    /// </summary>
    public int PerTenantConcurrency { get; init; } = 4;

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
