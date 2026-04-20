namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — dispatcher return envelope. <see cref="DedupHit"/> distinguishes a
/// first-time schedule from a no-op duplicate collapse (see
/// <c>R-OUT-EFF-IDEM-02</c>).
/// </summary>
public sealed record OutboundEffectScheduleResult(
    Guid EffectId,
    bool DedupHit);
