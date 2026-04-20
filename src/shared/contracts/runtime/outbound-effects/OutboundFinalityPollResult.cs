namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.4 — result returned by <see cref="IOutboundEffectAdapter.PollFinalityAsync"/>.
/// Sealed closed set so the finality poller can translate each variant into the
/// canonical lifecycle event deterministically.
/// </summary>
public abstract record OutboundFinalityPollResult
{
    private OutboundFinalityPollResult() { }

    /// <summary>
    /// Provider reports terminal success. Relay translates to
    /// <c>OutboundEffectFinalizedEvent(Succeeded, ...)</c>.
    /// </summary>
    public sealed record Succeeded(string EvidenceDigest) : OutboundFinalityPollResult;

    /// <summary>
    /// Provider reports terminal business failure. Relay translates to
    /// <c>OutboundEffectFinalizedEvent(BusinessFailed, ...)</c>.
    /// </summary>
    public sealed record BusinessFailed(string FailureCode, string EvidenceDigest)
        : OutboundFinalityPollResult;

    /// <summary>
    /// Provider reports partial completion (e.g., 4 of 5 recipients delivered).
    /// </summary>
    public sealed record PartiallyCompleted(string EvidenceDigest) : OutboundFinalityPollResult;

    /// <summary>
    /// Provider is still processing; poll again later. The poller keeps the row in
    /// <c>Acknowledged</c> status; the finality sweeper will eventually mark the
    /// effect reconciliation-required if the window expires.
    /// </summary>
    public sealed record StillPending : OutboundFinalityPollResult;

    /// <summary>
    /// Poll encountered a transient error; try again. NOT a classification of the
    /// provider's business outcome.
    /// </summary>
    public sealed record Transient(string Reason) : OutboundFinalityPollResult;

    /// <summary>
    /// Provider's state is un-pollable or unknown — reconciliation-required.
    /// </summary>
    public sealed record Unresolvable(string Reason) : OutboundFinalityPollResult;
}
