using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Whycespace.Platform.Host.Adapters.OutboundEffects;
using Whycespace.Shared.Contracts.Runtime.OutboundEffects;
using Xunit;

namespace Whycespace.Tests.Integration.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.2 — unit-level tests for <see cref="WebhookDeliveryAdapter"/>. Lives
/// under tests/integration because the adapter lives in
/// <c>Whycespace.Platform.Host</c> which the unit test project does not
/// reference. Uses a captured <see cref="HttpMessageHandler"/> to exercise
/// the HTTP wire without touching the network.
/// </summary>
public sealed class WebhookDeliveryAdapterTests
{
    private static readonly Guid EffectId = Guid.Parse("11111111-0000-0000-0000-000000000042");

    private static WebhookDeliveryOptions Options(string signingKey = "secret") => new()
    {
        SigningKey = signingKey,
    };

    private static OutboundEffectDispatchContext MakeContext(object payload) =>
        new(
            EffectId,
            new OutboundIdempotencyKey("Payout:42:notify"),
            AttemptNumber: 1,
            Payload: payload,
            CorrelationId: EffectId,
            CausationId: EffectId,
            ActorId: "system/outbound-effect-relay",
            DispatchTimeout: TimeSpan.FromSeconds(5));

    [Fact]
    public async Task Acknowledged_on_2xx_with_digest_derived_provider_op_id()
    {
        var handler = new CapturingHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent("{\"status\":\"ok\"}", Encoding.UTF8, "application/json");
            return response;
        });
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());
        var payload = new WebhookEffectPayload("https://partner.example.com/hook", "{\"id\":42}");

        var result = await adapter.DispatchAsync(MakeContext(payload), default);

        var acked = Assert.IsType<OutboundAdapterResult.Acknowledged>(result);
        Assert.Equal("http-webhook", acked.ProviderOperation.ProviderId);
        Assert.StartsWith("http-", acked.ProviderOperation.ProviderOperationId);
        Assert.Equal("Payout:42:notify", acked.ProviderOperation.IdempotencyKeyUsed);
        Assert.NotNull(acked.AckPayloadDigest);
    }

    [Fact]
    public async Task Acknowledged_prefers_response_X_Operation_Id_header_when_present()
    {
        var handler = new CapturingHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Headers.Add("X-Operation-Id", "partner-op-12345");
            response.Content = new StringContent("{}", Encoding.UTF8, "application/json");
            return response;
        });
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        var acked = Assert.IsType<OutboundAdapterResult.Acknowledged>(result);
        Assert.Equal("partner-op-12345", acked.ProviderOperation.ProviderOperationId);
    }

    [Fact]
    public async Task Idempotency_key_is_propagated_on_Idempotency_Key_header()
    {
        HttpRequestMessage? captured = null;
        var handler = new CapturingHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };
        });
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        Assert.NotNull(captured);
        Assert.True(captured!.Headers.TryGetValues("Idempotency-Key", out var values));
        Assert.Equal("Payout:42:notify", values!.Single());
    }

    [Fact]
    public async Task Hmac_signature_header_is_stable_across_retries_for_same_body()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        });
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options("key-xyz"));
        var payload = new WebhookEffectPayload("https://partner.example.com/hook", "{\"amount\":100}");

        await adapter.DispatchAsync(MakeContext(payload), default);
        var firstSig = handler.LastRequestHeaders?.GetValues("X-Whycespace-Signature").Single();

        await adapter.DispatchAsync(MakeContext(payload), default);
        var secondSig = handler.LastRequestHeaders?.GetValues("X-Whycespace-Signature").Single();

        Assert.NotNull(firstSig);
        Assert.Equal(firstSig, secondSig);
        Assert.StartsWith("sha256=", firstSig);
    }

    [Fact]
    public async Task Transient_on_5xx_with_retry_after()
    {
        var handler = new CapturingHandler(_ =>
        {
            var r = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(15));
            return r;
        });
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Transient, failed.Classification);
        Assert.Equal("http_503", failed.Reason);
        Assert.Equal(TimeSpan.FromSeconds(15), failed.RetryAfter);
    }

    [Fact]
    public async Task Terminal_on_4xx_except_408_429()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Terminal, failed.Classification);
        Assert.Equal("http_400", failed.Reason);
    }

    [Fact]
    public async Task Transient_on_429()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage((HttpStatusCode)429));
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Transient, failed.Classification);
    }

    [Fact]
    public async Task Terminal_on_malformed_target_url()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("not-a-url", "{}")), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Terminal, failed.Classification);
        Assert.StartsWith("target_url_malformed", failed.Reason);
    }

    [Fact]
    public async Task Terminal_on_payload_type_mismatch()
    {
        var handler = new CapturingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(payload: "a raw string, not a WebhookEffectPayload"), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Terminal, failed.Classification);
        Assert.StartsWith("payload_type_mismatch", failed.Reason);
    }

    [Fact]
    public async Task Transient_on_http_request_failure()
    {
        Func<HttpRequestMessage, HttpResponseMessage> throwing = _ => throw new HttpRequestException("dns failure");
        var handler = new CapturingHandler(throwing);
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var result = await adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), default);

        var failed = Assert.IsType<OutboundAdapterResult.DispatchFailedPreAcceptance>(result);
        Assert.Equal(OutboundAdapterClassification.Transient, failed.Classification);
        Assert.StartsWith("http_request_failed", failed.Reason);
    }

    [Fact]
    public async Task Cancellation_propagates_for_relay_to_classify()
    {
        // The adapter does not classify OCE — the relay's outer catch filters
        // on the original caller token to distinguish timeout vs host
        // shutdown. This test pins that the adapter lets OCE propagate.
        using var cts = new CancellationTokenSource();
        Func<HttpRequestMessage, Task<HttpResponseMessage>> slowResponse = async _ =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        };
        var handler = new CapturingHandler(slowResponse);
        var adapter = new WebhookDeliveryAdapter(new HttpClient(handler), Options());

        var dispatchTask = adapter.DispatchAsync(MakeContext(
            new WebhookEffectPayload("https://partner.example.com/hook", "{}")), cts.Token);

        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => dispatchTask);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _respond;

        public CapturingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond)
            : this(req => Task.FromResult(respond(req))) { }

        public CapturingHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
        {
            _respond = respond;
        }

        public HttpRequestHeaders? LastRequestHeaders { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestHeaders = request.Headers;
            var response = await _respond(request);
            response.RequestMessage = request;
            return response;
        }
    }
}
