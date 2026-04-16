using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.EventFabric;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// OPA-backed <see cref="IEnforcementEventEvaluator"/>. Sends each source
/// envelope to the rego package at <c>data.whyce.enforcement.detect</c> and
/// maps the structured response into <see cref="ViolationSignal"/> instances.
///
/// Request shape (OPA input):
/// <code>
/// {
///   "input": {
///     "event_type": "...",
///     "aggregate_id": "...",
///     "correlation_id": "...",
///     "payload": { ... raw payload ... }
///   }
/// }
/// </code>
///
/// Expected response shape:
/// <code>
/// {
///   "result": {
///     "signals": [
///       {
///         "rule_id": "guid",
///         "source_reference": "guid",
///         "reason": "…",
///         "severity": "Low|Medium|High|Critical",
///         "action": "Warn|Restrict|Block|Escalate"
///       }
///     ]
///   }
/// }
/// </code>
///
/// Absence of rego package, empty <c>signals</c>, or HTTP failure all collapse
/// to "no violations" (empty list) — this is the deny-by-default shape for a
/// detection evaluator (do not synthesize violations on infrastructure error).
/// Transport errors are logged at Warning and surfaced as an empty list so the
/// detection worker can commit the source offset.
/// </summary>
public sealed class OpaEnforcementEventEvaluator : IEnforcementEventEvaluator
{
    private static readonly IReadOnlyList<ViolationSignal> Empty = Array.Empty<ViolationSignal>();

    private readonly HttpClient _httpClient;
    private readonly string _evaluateUrl;
    private readonly int _requestTimeoutMs;
    private readonly ILogger<OpaEnforcementEventEvaluator>? _logger;

    public OpaEnforcementEventEvaluator(
        HttpClient httpClient,
        string opaEndpoint,
        int requestTimeoutMs,
        ILogger<OpaEnforcementEventEvaluator>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(opaEndpoint))
            throw new ArgumentException("opaEndpoint must be set.", nameof(opaEndpoint));
        if (requestTimeoutMs < 1)
            throw new ArgumentOutOfRangeException(nameof(requestTimeoutMs));

        _httpClient = httpClient;
        _evaluateUrl = $"{opaEndpoint.TrimEnd('/')}/v1/data/whyce/enforcement/detect";
        _requestTimeoutMs = requestTimeoutMs;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ViolationSignal>> EvaluateAsync(
        IEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            input = new
            {
                event_type = envelope.EventType,
                aggregate_id = envelope.AggregateId.ToString(),
                correlation_id = envelope.CorrelationId.ToString(),
                payload = envelope.Payload
            }
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromMilliseconds(_requestTimeoutMs));

        try
        {
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(_evaluateUrl, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning(
                    "OPA enforcement evaluation returned {StatusCode} for event {EventType}; no signals produced.",
                    (int)response.StatusCode, envelope.EventType);
                return Empty;
            }

            var body = await response.Content.ReadAsStringAsync(cts.Token);
            var opaResponse = JsonSerializer.Deserialize<OpaDetectResponse>(body);
            var signals = opaResponse?.Result?.Signals;
            if (signals is null || signals.Count == 0) return Empty;

            var mapped = new List<ViolationSignal>(signals.Count);
            foreach (var s in signals)
            {
                if (!Guid.TryParse(s.RuleId, out var ruleId)) continue;
                if (!Guid.TryParse(s.SourceReference, out var sourceRef)) continue;
                if (string.IsNullOrWhiteSpace(s.Reason)) continue;
                if (string.IsNullOrWhiteSpace(s.Severity)) continue;
                if (string.IsNullOrWhiteSpace(s.Action)) continue;

                mapped.Add(new ViolationSignal(
                    RuleId: ruleId,
                    SourceReference: sourceRef,
                    Reason: s.Reason,
                    Severity: s.Severity,
                    RecommendedAction: s.Action));
            }
            return mapped;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger?.LogWarning(
                "OPA enforcement evaluation timed out after {Timeout}ms for event {EventType}; no signals produced.",
                _requestTimeoutMs, envelope.EventType);
            return Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogWarning(ex,
                "OPA enforcement evaluation transport failure for event {EventType}; no signals produced.",
                envelope.EventType);
            return Empty;
        }
    }

    private sealed class OpaDetectResponse
    {
        [JsonPropertyName("result")]
        public OpaDetectResult? Result { get; set; }
    }

    private sealed class OpaDetectResult
    {
        [JsonPropertyName("signals")]
        public List<OpaDetectSignal>? Signals { get; set; }
    }

    private sealed class OpaDetectSignal
    {
        [JsonPropertyName("rule_id")]
        public string? RuleId { get; set; }

        [JsonPropertyName("source_reference")]
        public string? SourceReference { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }

        [JsonPropertyName("severity")]
        public string? Severity { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }
    }
}
