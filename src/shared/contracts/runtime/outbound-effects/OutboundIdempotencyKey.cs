namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-IDEM-01 — caller-supplied deterministic identity of an
/// outbound effect. Required (non-null, non-empty); capped at 255 chars to
/// match provider SDK conventions. Duplicate schedules with the same key
/// collapse to the existing <see cref="OutboundEffects.OutboundEffectScheduleResult"/>
/// with <c>DedupHit = true</c>.
///
/// Derivation convention (not enforced; caller's responsibility):
/// <c>{aggregateRootType}:{aggregateRootId}:{effectType}:{businessKey}</c>.
/// </summary>
public sealed record OutboundIdempotencyKey
{
    public string Value { get; }

    public OutboundIdempotencyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(OutboundEffectErrors.IdempotencyKeyRequired, nameof(value));
        if (value.Length > 255)
            throw new ArgumentException(OutboundEffectErrors.IdempotencyKeyTooLong, nameof(value));
        Value = value;
    }

    public override string ToString() => Value;
}
