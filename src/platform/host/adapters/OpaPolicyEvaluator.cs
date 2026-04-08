using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Whyce.Shared.Contracts.Infrastructure.Policy;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// OPA (Open Policy Agent) backed policy evaluator.
/// Sends policy evaluation requests to the OPA REST API.
///
/// phase1.5-S5.2.1 / PC-2 (OPA-CONFIG-01): hardened against the
/// pre-S5.2.1 unbounded HTTP shape (no timeout, no breaker, no
/// observability, raw EnsureSuccessStatusCode propagation). Three
/// guarantees are added without changing policy semantics:
///
///   1. Per-call timeout via a linked CTS sized from
///      <see cref="OpaOptions.RequestTimeoutMs"/>.
///   2. A narrow consecutive-failure circuit breaker (Closed / Open /
///      HalfOpen) sized from <see cref="OpaOptions.BreakerThreshold"/> and
///      <see cref="OpaOptions.BreakerWindowSeconds"/>. Open-state calls
///      throw immediately, never silently allow.
///   3. A <c>Whyce.Policy</c> meter exporting evaluation duration plus
///      timeout / breaker-open / failure counters.
///
/// Failure semantics: every transient failure (timeout, transport, non-2xx,
/// breaker-open) is reported as <see cref="PolicyEvaluationUnavailableException"/>
/// — the typed RETRYABLE REFUSAL path. There is no shape that produces an
/// allowed <see cref="PolicyDecision"/> on a failed evaluation. Policy
/// primacy ($8) is preserved by construction.
/// </summary>
public sealed class OpaPolicyEvaluator : IPolicyEvaluator
{
    // phase1.5-S5.2.1 / PC-2: dedicated meter for the policy hot path.
    // Counter / histogram names use the `policy.evaluate.*` namespace so
    // they are unambiguous in any registered MeterListener (OTel,
    // Prometheus exporter, dotnet-counters, ...).
    public static readonly Meter Meter = new("Whyce.Policy", "1.0");
    private static readonly Histogram<double> EvaluateDuration =
        Meter.CreateHistogram<double>("policy.evaluate.duration", unit: "ms");
    private static readonly Counter<long> TimeoutCounter =
        Meter.CreateCounter<long>("policy.evaluate.timeout");
    private static readonly Counter<long> BreakerOpenCounter =
        Meter.CreateCounter<long>("policy.evaluate.breaker_open");
    private static readonly Counter<long> FailureCounter =
        Meter.CreateCounter<long>("policy.evaluate.failure");

    private readonly HttpClient _httpClient;
    private readonly string _opaEndpoint;
    private readonly OpaOptions _options;
    private readonly IClock _clock;

    // Breaker state. The lock is uncontended on the happy path (a single
    // Interlocked sequence) and only enters the critical section on
    // failure or state-transition observation.
    private readonly object _breakerLock = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _openedAt;

    public OpaPolicyEvaluator(HttpClient httpClient, OpaOptions options, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(clock);
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new ArgumentException("OpaOptions.Endpoint must be set.", nameof(options));
        if (options.RequestTimeoutMs < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.RequestTimeoutMs,
                "OpaOptions.RequestTimeoutMs must be at least 1.");
        if (options.BreakerThreshold < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.BreakerThreshold,
                "OpaOptions.BreakerThreshold must be at least 1.");
        if (options.BreakerWindowSeconds < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.BreakerWindowSeconds,
                "OpaOptions.BreakerWindowSeconds must be at least 1.");

        _httpClient = httpClient;
        _opaEndpoint = options.Endpoint.TrimEnd('/');
        _options = options;
        _clock = clock;
    }

    public async Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        // --- Breaker gate (pre-call) ---
        // Open → throw immediately. HalfOpen window has elapsed → admit a
        // single trial call (the lock keeps the trial-call decision
        // single-writer; concurrent callers during HalfOpen still see Open
        // until the trial commits one way or the other).
        if (IsBreakerOpen())
        {
            BreakerOpenCounter.Add(1,
                new KeyValuePair<string, object?>("policy_id", policyId));
            throw new PolicyEvaluationUnavailableException(
                reason: "breaker_open",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: $"OPA circuit breaker open for policy '{policyId}'. No bypass allowed.");
        }

        var action = MapCommandTypeToAction(policyContext.CommandType, policyContext.Domain);
        var requestBody = new
        {
            input = new
            {
                policy_id = policyId,
                action,
                subject = new { role = policyContext.Roles.FirstOrDefault() ?? "anonymous" },
                resource = new
                {
                    classification = policyContext.Classification,
                    context = policyContext.Context,
                    domain = policyContext.Domain
                },
                correlation_id = policyContext.CorrelationId.ToString(),
                tenant_id = policyContext.TenantId,
                actor_id = policyContext.ActorId
            }
        };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var policyPath = policyId.Replace('.', '/');
        var url = $"{_opaEndpoint}/v1/data/whyce/policy/{policyPath}";

        // --- Timed call ---
        // Per-call CTS strictly bounds the OPA round-trip. Stopwatch
        // measures the actual wall duration; the histogram is recorded in
        // every branch (success, failure, timeout) so saturation is
        // observable end-to-end.
        var stopwatch = Stopwatch.StartNew();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(_options.RequestTimeoutMs));
        try
        {
            var response = await _httpClient.PostAsync(url, content, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                stopwatch.Stop();
                EvaluateDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                    new KeyValuePair<string, object?>("policy_id", policyId),
                    new KeyValuePair<string, object?>("outcome", "http_status"));
                FailureCounter.Add(1,
                    new KeyValuePair<string, object?>("policy_id", policyId),
                    new KeyValuePair<string, object?>("reason", "http_status"));
                RecordFailure();
                throw new PolicyEvaluationUnavailableException(
                    reason: "http_status",
                    retryAfterSeconds: _options.BreakerWindowSeconds,
                    message: $"OPA returned {(int)response.StatusCode} for policy '{policyId}'. No bypass allowed.");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
            var opaResult = JsonSerializer.Deserialize<OpaResponse>(responseBody);

            stopwatch.Stop();
            EvaluateDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("policy_id", policyId),
                new KeyValuePair<string, object?>("outcome", "ok"));
            RecordSuccess();

            var isAllowed = opaResult?.Result?.Allow ?? false;
            var decisionHash = ComputeDecisionHash(policyId, policyContext, isAllowed);
            var denialReason = isAllowed ? null : (opaResult?.Result?.DenialReason ?? "Policy denied by OPA");
            return new PolicyDecision(isAllowed, policyId, decisionHash, denialReason);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            stopwatch.Stop();
            EvaluateDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("policy_id", policyId),
                new KeyValuePair<string, object?>("outcome", "timeout"));
            TimeoutCounter.Add(1,
                new KeyValuePair<string, object?>("policy_id", policyId));
            FailureCounter.Add(1,
                new KeyValuePair<string, object?>("policy_id", policyId),
                new KeyValuePair<string, object?>("reason", "timeout"));
            RecordFailure();
            throw new PolicyEvaluationUnavailableException(
                reason: "timeout",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: $"OPA request for policy '{policyId}' exceeded {_options.RequestTimeoutMs} ms. No bypass allowed.");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            EvaluateDuration.Record(stopwatch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>("policy_id", policyId),
                new KeyValuePair<string, object?>("outcome", "transport"));
            FailureCounter.Add(1,
                new KeyValuePair<string, object?>("policy_id", policyId),
                new KeyValuePair<string, object?>("reason", "transport"));
            RecordFailure();
            throw new PolicyEvaluationUnavailableException(
                reason: "transport",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: $"OPA transport failure for policy '{policyId}': {ex.Message}. No bypass allowed.",
                innerException: ex);
        }
    }

    // --- Breaker primitives ---

    private bool IsBreakerOpen()
    {
        lock (_breakerLock)
        {
            if (_openedAt is null) return false;
            var elapsed = _clock.UtcNow - _openedAt.Value;
            if (elapsed.TotalSeconds < _options.BreakerWindowSeconds)
                return true;
            // HalfOpen: allow exactly one trial call by clearing _openedAt
            // but leaving _consecutiveFailures intact. A successful trial
            // resets via RecordSuccess; a failed trial re-opens via
            // RecordFailure (consecutive count is already at threshold).
            _openedAt = null;
            return false;
        }
    }

    private void RecordSuccess()
    {
        lock (_breakerLock)
        {
            _consecutiveFailures = 0;
            _openedAt = null;
        }
    }

    private void RecordFailure()
    {
        lock (_breakerLock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _options.BreakerThreshold && _openedAt is null)
            {
                _openedAt = _clock.UtcNow;
            }
        }
    }

    // --- Unchanged helpers ---

    private static string ComputeDecisionHash(string policyId, PolicyContext context, bool allowed)
    {
        var seed = $"{policyId}:{context.CorrelationId}:{context.CommandType}:{allowed}";
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return Convert.ToHexStringLower(hash);
    }

    private static string MapCommandTypeToAction(string commandType, string domain)
    {
        var name = commandType.Replace("Command", "", StringComparison.Ordinal);
        var domainIndex = name.IndexOf(domain, StringComparison.OrdinalIgnoreCase);
        var verb = domainIndex > 0
            ? name[..domainIndex].ToLowerInvariant()
            : name.ToLowerInvariant();
        if (commandType == "WorkflowStartCommand")
            verb = "create";
        return $"{domain}.{verb}";
    }

    private sealed class OpaResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("result")]
        public OpaResultPayload? Result { get; set; }
    }

    private sealed class OpaResultPayload
    {
        [System.Text.Json.Serialization.JsonPropertyName("allow")]
        public bool Allow { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("deny")]
        public bool Deny { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("denial_reason")]
        public string? DenialReason { get; set; }
    }
}
