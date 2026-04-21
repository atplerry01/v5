namespace Whycespace.Domain.ContentSystem.Invariant.BroadcastStreamBinding;

/// <summary>
/// Cross-system invariant: every Broadcast (live-streaming/broadcast) must
/// reference a Stream (stream-core/stream) that exists and is not in a
/// terminal state (Ended / Archived).
///
/// Spans two BCs in the streaming context. Lives here, not on the aggregate,
/// because the authoritative Stream state cannot be known from Broadcast
/// alone — the caller must load the Stream projection fact before creating
/// or starting the Broadcast.
///
/// Pure decision function per 02-engine-skeleton.md § Cross-System
/// Invariants. Intended to be invoked in T2E handlers (CreateBroadcastHandler,
/// StartBroadcastHandler) before aggregate mutation.
/// </summary>
public sealed class BroadcastStreamBindingPolicy
{
    public BroadcastStreamBindingDecision Evaluate(BroadcastStreamBindingInput input)
    {
        if (input.StreamId == Guid.Empty)
            return BroadcastStreamBindingDecision.Deny(BroadcastStreamBindingReason.MissingStream);

        if (!input.StreamExists)
            return BroadcastStreamBindingDecision.Deny(BroadcastStreamBindingReason.StreamNotFound);

        if (input.StreamStatusIsTerminal)
            return BroadcastStreamBindingDecision.Deny(BroadcastStreamBindingReason.StreamTerminal);

        return BroadcastStreamBindingDecision.Allow();
    }
}
