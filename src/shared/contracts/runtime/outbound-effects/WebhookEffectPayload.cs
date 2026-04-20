namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.2 — payload carried by outbound-effect intents targeting the HTTP
/// webhook delivery adapter (<c>ProviderId = "http-webhook"</c>). The payload
/// is persisted on the <c>OutboundEffectScheduledEvent</c> stream and
/// replayed on retry; it MUST be deterministic (no embedded timestamps or
/// random values).
///
/// <para>The adapter POSTs <see cref="Body"/> (UTF-8) to <see cref="TargetUrl"/>
/// with a <c>Content-Type: application/json</c> header, the
/// <c>Idempotency-Key</c> header set from the effect's
/// <see cref="OutboundIdempotencyKey"/>, an HMAC-SHA256 signature header
/// (per <c>WebhookDeliveryOptions</c>), and any explicit additional
/// headers supplied here.</para>
/// </summary>
public sealed record WebhookEffectPayload(
    string TargetUrl,
    string Body,
    IReadOnlyDictionary<string, string>? AdditionalHeaders = null);
