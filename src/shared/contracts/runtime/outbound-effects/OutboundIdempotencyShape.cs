namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 — classifies how a provider handles duplicate outbound calls. Every
/// <see cref="IOutboundEffectAdapter"/> declares its shape so the relay can
/// decide whether post-dispatch retries are safe (see
/// <c>R-OUT-EFF-IDEM-05</c>).
/// </summary>
public enum OutboundIdempotencyShape
{
    /// <summary>Provider accepts an idempotency key that we send (Stripe, Square).</summary>
    ProviderIdempotent,

    /// <summary>Provider treats repeated call with same natural business key as no-op (upsert-by-ref).</summary>
    NaturalKeyIdempotent,

    /// <summary>Provider has no idempotency surface. Duplicate = duplicate (SMS, legacy bank transfer).</summary>
    AtMostOnceRequired,

    /// <summary>Duplicate is possible but reversible via compensation (bank reversal, refund).</summary>
    CompensatableOnly,
}
