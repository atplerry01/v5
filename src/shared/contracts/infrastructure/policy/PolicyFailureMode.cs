namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

/// <summary>
/// R2.A.2 — canonical classification of an <see cref="IPolicyEvaluator"/>
/// failure per POL-FAIL-CLASS-01. When an evaluator cannot produce a
/// deterministic allow/deny decision (OPA outage, timeout, network
/// error), the adapter MUST return a <see cref="PolicyDecision"/> with
/// <see cref="PolicyDecision.FailureMode"/> set to one of these values.
///
/// <list type="bullet">
///   <item><b><see cref="FailClosed"/></b> — reject the command. Default for
///         all production environments. Emits <c>PolicyEvaluationFailedEvent</c>.
///         No retry.</item>
///   <item><b><see cref="FailOpen"/></b> — permit the command under an explicit,
///         audited degraded posture. Only valid when the policy itself declares
///         fail-open-eligibility (breaking-glass / emergency paths). Emits
///         <c>PolicyBypassedEvent</c> with reason. Never the default.</item>
///   <item><b><see cref="Defer"/></b> — retry with bounded attempts via
///         <c>IRetryExecutor</c>. Maps to
///         <c>RuntimeFailureCategory.PolicyEvaluationDeferred</c> which
///         <c>RetryEligibility</c> treats as retryable. On the final attempt
///         a DEFER converts to a terminal <see cref="FailClosed"/>.</item>
/// </list>
///
/// A genuine deny (evaluator ran successfully and said "no") does NOT carry a
/// <see cref="PolicyFailureMode"/> — it remains an allow/deny decision with
/// <c>IsAllowed = false</c>.
/// </summary>
public enum PolicyFailureMode
{
    /// <summary>Reject the command. Default for all production environments.</summary>
    FailClosed = 0,

    /// <summary>Permit under explicit audited degraded posture. Never default.</summary>
    FailOpen = 1,

    /// <summary>Retryable with bounded attempts. Converts to FailClosed on exhaustion.</summary>
    Defer = 2
}
