namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — terminal business outcome on <c>Finalized</c> and <c>Reconciled</c>
/// transitions. Replay-stable discriminator carried on the aggregate event
/// stream.
/// </summary>
public enum OutboundFinalityOutcome
{
    /// <summary>Provider reports success.</summary>
    Succeeded,

    /// <summary>Provider reports terminal failure (insufficient funds, declined, etc.).</summary>
    BusinessFailed,

    /// <summary>Provider reports partial success (e.g., 4 of 5 recipients delivered).</summary>
    PartiallyCompleted,

    /// <summary>Reconciliation could not resolve; human-owned terminal state.</summary>
    ManualIntervention,
}
