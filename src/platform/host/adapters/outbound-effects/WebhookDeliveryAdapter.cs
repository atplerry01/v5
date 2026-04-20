using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;

namespace Whycespace.Platform.Host.Adapters.OutboundEffects;

/// <summary>
/// R3.B.2 — first real outbound-effect adapter. Delivers an HTTP webhook to a
/// caller-supplied URL (<see cref="WebhookEffectPayload.TargetUrl"/>) with an
/// HMAC-SHA256 signature header plus the effect's idempotency key on the
/// <c>Idempotency-Key</c> header (<c>R-OUT-EFF-IDEM-03</c>).
///
/// <para><b>Idempotency shape:</b>
/// <see cref="OutboundIdempotencyShape.ProviderIdempotent"/> — partners that
/// honor the <c>Idempotency-Key</c> header collapse retries server-side
/// (industry convention used by Stripe, Square, Slack retry semantics, and
/// most modern webhook receivers). The relay reuses the effect's key across
/// retry attempts (<c>R-OUT-EFF-IDEM-04</c>).</para>
///
/// <para><b>Finality strategy:</b>
/// <see cref="OutboundFinalityStrategy.ManualOnly"/> for R3.B.2 —
/// acknowledgement is transport-level (HTTP 2xx). Push-mode and Poll-mode
/// finality land in R3.B.4 (inbound webhook callback + scheduled poller).</para>
///
/// <para><b>Classification:</b>
/// <list type="bullet">
///   <item>HTTP 2xx → <see cref="OutboundAdapterResult.Acknowledged"/> carrying
///         a <see cref="ProviderOperationIdentity"/> derived from the response
///         (either the <c>X-Operation-Id</c> header if the target returns it
///         or a SHA-256 digest of the response body).</item>
///   <item>HTTP 4xx (except 408, 429) → <see cref="OutboundAdapterResult.DispatchFailedPreAcceptance"/>
///         with <see cref="OutboundAdapterClassification.Terminal"/>.</item>
///   <item>HTTP 408 / 429 / 5xx → Transient with <c>Retry-After</c> (if any)
///         honored by the relay.</item>
///   <item>Timeout (linked-CTS cancellation, NOT caller-cancellation) →
///         Transient with reason <c>dispatch_timeout</c>.</item>
///   <item>Network error / DNS failure → Transient.</item>
///   <item>Caller cancellation → <see cref="OperationCanceledException"/>
///         propagates — the relay treats it as host-shutdown and re-claims
///         the row on next boot.</item>
/// </list></para>
///
/// <para><b>R-OUT-EFF-PROHIBITION-01:</b> the <see cref="HttpClient"/> injected
/// here is the ONLY non-OPA HttpClient consumer in the solution. The
/// <c>HttpClient_Usage_Confined_To_Whitelist</c> architecture test pins it.</para>
///
/// <para><b>R-OUT-EFF-DET-01:</b> this file references no <c>Guid.NewGuid()</c>,
/// no <c>DateTime.UtcNow</c>, no <c>Random</c>. Timing decisions live in the
/// relay's deterministic retry path.</para>
/// </summary>
public sealed class WebhookDeliveryAdapter : IOutboundEffectAdapter
{
    private readonly HttpClient _httpClient;
    private readonly WebhookDeliveryOptions _options;
    private readonly ILogger<WebhookDeliveryAdapter>? _logger;

    public WebhookDeliveryAdapter(
        HttpClient httpClient,
        WebhookDeliveryOptions options,
        ILogger<WebhookDeliveryAdapter>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public string ProviderId => _options.ProviderId;
    public OutboundIdempotencyShape IdempotencyShape => OutboundIdempotencyShape.ProviderIdempotent;
    public OutboundFinalityStrategy FinalityStrategy => OutboundFinalityStrategy.ManualOnly;

    public async Task<OutboundAdapterResult> DispatchAsync(
        OutboundEffectDispatchContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Payload is not WebhookEffectPayload payload)
        {
            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Terminal,
                $"payload_type_mismatch:expected WebhookEffectPayload, got {context.Payload.GetType().Name}");
        }

        if (string.IsNullOrWhiteSpace(payload.TargetUrl))
        {
            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Terminal,
                "target_url_missing");
        }

        if (!Uri.TryCreate(payload.TargetUrl, UriKind.Absolute, out var targetUri))
        {
            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Terminal,
                $"target_url_malformed:{payload.TargetUrl}");
        }

        if (!_options.AllowedSchemes.Any(s =>
            targetUri.AbsoluteUri.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
        {
            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Terminal,
                $"scheme_not_allowed:{targetUri.Scheme}");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, targetUri);
        var bodyBytes = Encoding.UTF8.GetBytes(payload.Body ?? string.Empty);
        request.Content = new ByteArrayContent(bodyBytes);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // R-OUT-EFF-IDEM-03: propagate the effect's idempotency key as the
        // provider Idempotency-Key header so partner-side dedup can collapse
        // retries. The value is the local OutboundIdempotencyKey verbatim —
        // relay guarantees stability across retries via R-OUT-EFF-IDEM-04.
        request.Headers.TryAddWithoutValidation("Idempotency-Key", context.IdempotencyKey.Value);
        request.Headers.TryAddWithoutValidation(_options.EffectIdHeader, context.EffectId.ToString());

        // HMAC-SHA256 signature over the body bytes. Deterministic given
        // (signing_key, body) — no entropy or clock reads.
        var signatureValue = ComputeSignature(_options.SigningKey, bodyBytes);
        request.Headers.TryAddWithoutValidation(
            _options.SignatureHeader,
            $"sha256={signatureValue}");

        if (payload.AdditionalHeaders is { } extras)
        {
            foreach (var (name, value) in extras)
            {
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cancellation here can be either the caller's (host shutdown) or
            // the relay's per-attempt linked CTS (dispatch timeout). The
            // adapter cannot distinguish — the relay's outer catch filters on
            // the original caller token to classify timeout vs shutdown. Re-
            // throw so the relay decides.
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogWarning(ex,
                "WebhookDeliveryAdapter HTTP error effectId={EffectId} url={Url}",
                context.EffectId, targetUri);
            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Transient,
                $"http_request_failed:{ex.GetType().Name}:{ex.Message}");
        }

        using (response)
        {
            return await TranslateAsync(response, cancellationToken);
        }
    }

    private async Task<OutboundAdapterResult> TranslateAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var statusCode = (int)response.StatusCode;

        if (response.IsSuccessStatusCode)
        {
            var providerOpId = response.Headers.TryGetValues("X-Operation-Id", out var ids)
                ? ids.FirstOrDefault()
                : null;

            var bodyBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var digest = Convert.ToHexString(SHA256.HashData(bodyBytes)).ToLowerInvariant();

            var identity = new ProviderOperationIdentity(
                ProviderId: ProviderId,
                ProviderOperationId: string.IsNullOrWhiteSpace(providerOpId) ? $"http-{digest[..16]}" : providerOpId,
                IdempotencyKeyUsed: response.RequestMessage?.Headers.TryGetValues("Idempotency-Key", out var used) == true
                    ? used.FirstOrDefault()
                    : null);

            return new OutboundAdapterResult.Acknowledged(identity, AckPayloadDigest: digest);
        }

        // Transient statuses — partner indicated temporary unavailability.
        if (statusCode == 408 || statusCode == 429 || statusCode >= 500)
        {
            TimeSpan? retryAfter = null;
            if (response.Headers.RetryAfter?.Delta is { } delta) retryAfter = delta;

            return new OutboundAdapterResult.DispatchFailedPreAcceptance(
                OutboundAdapterClassification.Transient,
                $"http_{statusCode}",
                retryAfter);
        }

        // 4xx (except 408/429) — terminal; partner rejected the payload.
        return new OutboundAdapterResult.DispatchFailedPreAcceptance(
            OutboundAdapterClassification.Terminal,
            $"http_{statusCode}");
    }

    private static string ComputeSignature(string signingKey, byte[] body)
    {
        var keyBytes = Encoding.UTF8.GetBytes(signingKey ?? string.Empty);
        using var hmac = new HMACSHA256(keyBytes);
        var sig = hmac.ComputeHash(body);
        return Convert.ToHexString(sig).ToLowerInvariant();
    }
}
