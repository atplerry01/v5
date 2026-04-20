namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / ratified constraint #2 — named-field provider operation identity.
/// First-class as of R3.B.1; never free-text observability. Carried on
/// <c>OutboundEffectAcknowledgedEvent</c>, <c>OutboundEffectFinalizedEvent</c>,
/// and inbound callback correlation.
/// </summary>
/// <param name="ProviderId">
/// Duplicated here for safety across system boundaries — the consumer of the
/// identity record should not have to re-look-up which provider originated it.
/// </param>
/// <param name="ProviderOperationId">
/// Opaque provider-assigned identifier. Stable across retries when the
/// provider treats the call as idempotent; otherwise may change per attempt.
/// </param>
/// <param name="IdempotencyKeyUsed">
/// The key we actually sent to the provider (may differ in format from the
/// local <see cref="OutboundIdempotencyKey"/>). Null when the adapter does not
/// propagate a key (e.g., <c>AtMostOnceRequired</c> providers).
/// </param>
public sealed record ProviderOperationIdentity(
    string ProviderId,
    string ProviderOperationId,
    string? IdempotencyKeyUsed = null);
