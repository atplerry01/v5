namespace Whycespace.Shared.Contracts.Runtime.OutboundEffects;

/// <summary>
/// R3.B.4 — input record for <c>WebhookCallbackIngressHandler</c>. Composes
/// the caller-supplied path/effect id, the body bytes we verify the HMAC
/// signature against, the signature header value, and the provider's
/// declared operation id (body-derived or header-derived depending on the
/// provider). The handler refuses ingress when any field is missing or
/// mismatched with the stored aggregate.
/// </summary>
public sealed record WebhookCallbackIngressRequest(
    Guid EffectId,
    string ProviderId,
    string ProviderOperationId,
    string SignatureHeader,
    byte[] Body,
    string OutcomeCode,
    string EvidenceDigest);

/// <summary>
/// R3.B.4 — canonical outcome of a single <c>WebhookCallbackIngressHandler</c>
/// invocation. Callers (HTTP endpoint) translate to 2xx/4xx responses.
/// </summary>
public enum WebhookIngressOutcome
{
    /// <summary>Callback accepted; Finalized event emitted.</summary>
    Finalized,

    /// <summary>Callback accepted; Reconciled event emitted.</summary>
    Reconciled,

    /// <summary>Callback rejected due to signature mismatch.</summary>
    SignatureInvalid,

    /// <summary>Callback rejected because no matching effect exists (orphan).</summary>
    UnknownEffect,

    /// <summary>Callback correlates to an effect but ProviderOperationId mismatches.</summary>
    ProviderOperationIdMismatch,

    /// <summary>Callback correlates but the aggregate is in a status that rejects finality (e.g. Finalized already).</summary>
    InvalidStatus,
}
