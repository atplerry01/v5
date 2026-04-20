namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R1 §5 — dependency readiness check seam. Registered per command type
/// (or globally). Called by the admission layer BEFORE policy / authorization
/// to short-circuit commands whose required downstream dependencies are
/// not ready.
///
/// This is DISTINCT from:
/// - Liveness / readiness probes (infrastructure layer, Kubernetes-style)
/// - Runtime degraded-mode posture (already surfaced via <c>CommandContext.DegradedMode</c>)
/// - Circuit breakers (R2 — protects callers from slow dependencies mid-flight)
///
/// Readiness here means: "the dependencies this specific command needs to
/// complete are currently available". A failed readiness check produces
/// <see cref="ValidationFailureCategory.DependencyReadiness"/>.
///
/// Implementations MUST be fast (sub-millisecond) and side-effect-free.
/// They typically read an in-memory health snapshot, not a live probe.
/// </summary>
public interface IDependencyReadinessCheck
{
    /// <summary>Command type this check is bound to. Null = applies to all commands.</summary>
    Type? CommandType { get; }

    /// <summary>
    /// Returns a valid <see cref="ValidationResult"/> when all required
    /// dependencies are ready, or an invalid result with category
    /// <see cref="ValidationFailureCategory.DependencyReadiness"/> otherwise.
    /// </summary>
    ValueTask<ValidationResult> CheckAsync(object command, CancellationToken cancellationToken = default);
}
