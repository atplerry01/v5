namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.2 — per-adapter options for the HTTP Webhook Delivery adapter. Reads
/// from <c>OutboundEffects:WebhookDelivery:*</c>. Required fields throw at
/// composition when missing.
///
/// <para>Signing: every outbound request carries an HMAC-SHA256 signature
/// header (default <c>X-Whycespace-Signature</c>) whose value is
/// <c>sha256={hex(hmac_sha256(signing_key, body))}</c>. Signing key MUST
/// NOT be empty in production. The signing key is typed here as a plain
/// string; the composition-root wiring is expected to source it from secret
/// configuration, not from public <c>appsettings.json</c>.</para>
/// </summary>
public sealed record WebhookDeliveryOptions
{
    /// <summary>Stable provider id — matches <see cref="IOutboundEffectAdapter.ProviderId"/>.</summary>
    public string ProviderId { get; init; } = "http-webhook";

    /// <summary>HMAC-SHA256 signing key. Required in production; may be empty in dev/test.</summary>
    public required string SigningKey { get; init; }

    /// <summary>Header carrying the HMAC signature. Defaults to <c>X-Whycespace-Signature</c>.</summary>
    public string SignatureHeader { get; init; } = "X-Whycespace-Signature";

    /// <summary>Header carrying the event id. Defaults to <c>X-Whycespace-Effect-Id</c>.</summary>
    public string EffectIdHeader { get; init; } = "X-Whycespace-Effect-Id";

    /// <summary>Optional allow-list of permitted scheme prefixes (e.g. <c>https://</c>).</summary>
    public IReadOnlyList<string> AllowedSchemes { get; init; } = new[] { "https://", "http://" };
}
