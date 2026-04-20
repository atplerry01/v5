namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — canonical causes for entering the
/// <c>ReconciliationRequired</c> lifecycle state. Audit-only; not a replay
/// discriminator.
/// </summary>
public enum OutboundReconciliationCause
{
    /// <summary>Dispatched but no provider ack arrived within AckTimeoutMs.</summary>
    AckTimeoutExpired,

    /// <summary>Acknowledged but no business finality arrived within FinalityWindowMs.</summary>
    FinalityTimeoutExpired,

    /// <summary>Poll or callback disagrees with our expected outcome.</summary>
    ProviderDisagreement,

    /// <summary>HTTP-level ambiguity (timeout mid-response, read-side ack failure).</summary>
    DispatchAmbiguous,
}
