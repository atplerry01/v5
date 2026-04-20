namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — three-way failure classification emitted by adapter results. The
/// relay consults this plus the adapter's <see cref="OutboundIdempotencyShape"/>
/// to decide retry vs reconciliation vs terminal refusal. Mirrors the shape of
/// <c>WorkflowStepFailureClassification</c> for operator familiarity.
/// </summary>
public enum OutboundAdapterClassification
{
    /// <summary>Retryable (timeout, 5xx, network error, breaker-open).</summary>
    Transient,

    /// <summary>Non-retryable (4xx validation, policy reject, malformed payload).</summary>
    Terminal,

    /// <summary>Ambiguous dispatch; retry is DANGEROUS for AtMostOnceRequired adapters.</summary>
    Ambiguous,
}
