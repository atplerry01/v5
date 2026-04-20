namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — declares how an adapter obtains business finality after the
/// provider has acknowledged an operation. Declared per adapter; R3.B.1 ships
/// the enum and the relay honors <see cref="ManualOnly"/> behavior. Push,
/// Poll, Hybrid become operative in R3.B.4.
/// </summary>
public enum OutboundFinalityStrategy
{
    /// <summary>Finality arrives via inbound webhook/callback. Poller not scheduled.</summary>
    Push,

    /// <summary>Finality is polled at scheduled intervals after Acknowledged.</summary>
    Poll,

    /// <summary>Push first; poller activates after AckTimeoutMs silence.</summary>
    Hybrid,

    /// <summary>No automated finality resolution; manual reconciliation required for every ack.</summary>
    ManualOnly,
}
