namespace Whycespace.Shared.Kernel.Domain;

/// <summary>
/// Shared-layer marker interface denoting "this exception represents a
/// domain invariant violation". Enables adapter layers (HTTP edge,
/// messaging DLQ) to recognise invariant-violation exceptions WITHOUT
/// taking a reference on <c>Whycespace.Domain</c>.
///
/// <para>
/// Introduced 2026-04-20 to remediate <c>DG-BASELINE-01</c> / <c>DG-R5-01</c>:
/// <c>src/platform/api</c> previously referenced <c>Whycespace.Domain</c>
/// to catch <c>DomainException</c> at the HTTP edge. Per the
/// <c>platform_api</c> layer rule (<c>dependency-check.sh</c> ALLOWED
/// matrix) that layer may only reference <c>Whycespace.Systems</c> and
/// <c>Whycespace.Shared</c>. This marker interface lets
/// <c>DomainException</c> remain in its canonical domain location while
/// the API middleware catches by interface — restoring layer purity
/// without changing external behaviour.
/// </para>
///
/// <para>
/// <b>Contract:</b> implementers MUST be <see cref="System.Exception"/>
/// subtypes whose raising represents a detected violation of a
/// declared domain-level invariant (aggregate rule, value-object
/// precondition, bounded-context constraint). Adapter layers MAY map
/// all such exceptions to a uniform failure-at-caller response
/// (e.g. HTTP 400 Bad Request, RFC 7807 ProblemDetails) without
/// inspecting the concrete exception type.
/// </para>
///
/// <para>
/// <b>Not for:</b> runtime / infrastructure failures (timeouts,
/// breaker-open, DB transport errors, policy-evaluation-deferred).
/// Those flow through <c>RuntimeFailureCategory</c> and
/// <c>RuntimeExceptionMapper</c> at the runtime layer, not through
/// this marker.
/// </para>
/// </summary>
public interface IDomainInvariantViolation
{
}
