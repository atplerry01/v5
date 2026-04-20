namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / ratified constraint #1 — six-outcome adapter result model. The
/// variants are exhaustive; adapters that attempt to collapse cases (e.g.,
/// returning <see cref="Acknowledged"/> when finality was actually achieved)
/// are a <c>R-OUT-EFF-FINALITY-01</c> violation. The abstract base is sealed
/// by virtue of its nested sealed records — no third-party variants.
/// </summary>
public abstract record OutboundAdapterResult
{
    private OutboundAdapterResult() { }

    /// <summary>
    /// 1. Dispatch failed before the provider accepted anything. No provider
    /// operation id. <see cref="Classification"/> drives retry:
    /// <see cref="OutboundAdapterClassification.Transient"/> → retry;
    /// <see cref="OutboundAdapterClassification.Terminal"/> → stop.
    /// </summary>
    public sealed record DispatchFailedPreAcceptance(
        OutboundAdapterClassification Classification,
        string Reason,
        TimeSpan? RetryAfter = null) : OutboundAdapterResult;

    /// <summary>
    /// 2. Transport-level dispatch succeeded but provider returned no operation
    /// id. Signals ambiguous state. Triggers reconciliation on finality timeout.
    /// </summary>
    public sealed record DispatchedWithoutProviderOperationId(
        string TransportEvidence) : OutboundAdapterResult;

    /// <summary>
    /// 3. Provider accepted and returned an operation id. Business finality is
    /// NOT implied — must transition to <see cref="FinalizedSuccess"/> or
    /// <see cref="FinalizedFailure"/> via subsequent callback / poll /
    /// reconciliation ritual.
    /// </summary>
    public sealed record Acknowledged(
        ProviderOperationIdentity ProviderOperation,
        string? AckPayloadDigest = null) : OutboundAdapterResult;

    /// <summary>
    /// 4. Provider returned synchronous terminal success. Business finality
    /// achieved in-call (rare — only for simple synchronous providers).
    /// </summary>
    public sealed record FinalizedSuccess(
        ProviderOperationIdentity ProviderOperation,
        string FinalityEvidence) : OutboundAdapterResult;

    /// <summary>
    /// 5. Provider returned synchronous terminal failure. Business finality
    /// achieved (card declined, recipient invalid, etc.). Triggers compensation
    /// path when a compensation workflow is registered (R3.B.5 scope).
    /// </summary>
    public sealed record FinalizedFailure(
        ProviderOperationIdentity? ProviderOperation,
        string FailureCode,
        string FailureReason) : OutboundAdapterResult;

    /// <summary>
    /// 6. Provider state unclear. Adapter cannot reliably determine outcome
    /// (e.g., timeout mid-response on at-most-once adapter). Reconciliation
    /// required.
    /// </summary>
    public sealed record ReconciliationRequired(
        OutboundReconciliationCause Cause,
        string DiagnosticEvidence) : OutboundAdapterResult;
}
