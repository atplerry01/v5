using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Whycespace.Shared.Contracts.Infrastructure.Policy;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Platform.Host.Adapters;

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
///   3. A <c>Whycespace.Policy</c> meter exporting evaluation duration plus
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
    public static readonly Meter Meter = new("Whycespace.Policy", "1.0");
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
        if (IsBreakerOpenInternal())
        {
            BreakerOpenCounter.Add(1,
                new KeyValuePair<string, object?>("policy_id", policyId));
            throw new PolicyEvaluationUnavailableException(
                reason: "breaker_open",
                retryAfterSeconds: _options.BreakerWindowSeconds,
                message: $"OPA circuit breaker open for policy '{policyId}'. No bypass allowed.");
        }

        var action = MapCommandTypeToAction(policyContext.CommandType, policyContext.Domain);
        var policyPath = BuildOpaPackagePath(policyId);
        var url = string.IsNullOrEmpty(policyPath)
            ? $"{_opaEndpoint}/v1/data/whyce/policy"
            : $"{_opaEndpoint}/v1/data/whyce/policy/{policyPath}";

        // Per-role iteration. The rego policies gate on
        // `input.subject.role == "<singular>"`, so a caller carrying multiple
        // roles (`["admin","operator"]`) is evaluated once per role. The
        // decision is ALLOW if ANY role satisfies the policy; otherwise the
        // last denial is returned so the denial reason remains attributable.
        //
        // Order independence: the iteration order over the Roles array is
        // deterministic and the short-circuit is ALLOW-on-first-match, so the
        // final outcome is invariant under input ordering — any permutation
        // of the same role set produces the same allow/deny decision.
        var roles = policyContext.Roles is { Length: > 0 }
            ? policyContext.Roles
            : new[] { "anonymous" };

        PolicyDecision? lastDenial = null;
        foreach (var role in roles)
        {
            var decision = await EvaluateSingleAsync(
                policyId, policyPath, url, action, role, policyContext);

            if (decision.IsAllowed)
                return decision;

            lastDenial = decision;
        }

        return lastDenial!;
    }

    private async Task<PolicyDecision> EvaluateSingleAsync(
        string policyId,
        string policyPath,
        string url,
        string action,
        string role,
        PolicyContext policyContext)
    {
        // Build the `input.subject` object: the rego-expected singular `role`
        // plus any typed attribute claims forwarded by
        // ICallerIdentityAccessor (kyc_attestation_present, trust_score, …).
        // Absent attributes are OMITTED rather than defaulted, so a missing
        // claim cannot satisfy a rego `== true` / `>= floor` check — this
        // preserves the deny-by-default contract.
        var subject = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["role"] = role,
        };
        if (policyContext.SubjectAttributes is { Count: > 0 } attrs)
        {
            foreach (var kv in attrs)
            {
                // Never allow an attribute to overwrite the canonical `role`
                // key; caller-supplied `role` in the attribute dictionary is
                // silently ignored.
                if (kv.Key == "role") continue;
                subject[kv.Key] = kv.Value;
            }
        }

        var requestBody = new
        {
            input = new
            {
                policy_id = policyId,
                action,
                subject,
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

    /// <summary>
    /// phase1.5-S5.2.4 / HC-2 (RUNTIME-STATE-AGGREGATION-01):
    /// side-effect-free public getter for the canonical
    /// runtime-state aggregator. Returns <c>true</c> when the
    /// breaker is currently in the Open window (i.e.
    /// <c>_openedAt</c> is set). Does NOT perform the HalfOpen
    /// transition that the private call-site
    /// <see cref="IsBreakerOpenInternal()"/> does — that side-effect is
    /// reserved for the request-path call site so the aggregator
    /// poll cannot accidentally admit a trial call.
    /// </summary>
    public bool IsBreakerOpen
    {
        get
        {
            lock (_breakerLock)
            {
                return _openedAt is not null;
            }
        }
    }

    private bool IsBreakerOpenInternal()
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

    /// <summary>
    /// Translates a canonical policy id (e.g. <c>whyce.economic.capital.account.open</c>)
    /// into the OPA data-path segment that resolves to the matching rego
    /// package — for the example above, <c>economic/capital/account</c>, which
    /// the caller appends to <c>/v1/data/whyce/policy/</c> so OPA evaluates
    /// <c>data.whyce.policy.economic.capital.account</c> (the package
    /// containing <c>allow</c> rules keyed on <c>input.policy_id</c>).
    ///
    /// Logic:
    ///   1. Strip the leading <c>whyce.</c> prefix once (and only the literal
    ///      dotted prefix — the legacy fallback id <c>whyce-policy-default</c>
    ///      keeps its hyphen and is preserved verbatim for backward compat).
    ///   2. Drop the trailing action segment — the action remains available to
    ///      rego via <c>input.policy_id</c>, not via the URL suffix.
    ///   3. Replace remaining dots with slashes.
    ///   4. Returns an empty string for ids that have no dotted structure
    ///      after step 1; the caller treats empty as "query the base
    ///      <c>whyce.policy</c> package".
    /// </summary>
    internal static string BuildOpaPackagePath(string policyId)
    {
        if (string.IsNullOrEmpty(policyId)) return string.Empty;

        // Backward-compat: the legacy default sentinel is not a dotted policy
        // id and must not be reshaped — preserve historical behaviour for any
        // command that hasn't yet been bound via ICommandPolicyIdRegistry.
        const string LegacyDefault = "whyce-policy-default";
        if (policyId == LegacyDefault) return policyId;

        // Only the canonical "whyce.<class>.<ctx>.<domain>.<action>" shape is
        // reshaped. Any other dotted id is treated as opaque and passed through
        // with the historical 1:1 dot-to-slash behaviour so this fix does not
        // surprise any existing non-canonical wiring.
        const string Prefix = "whyce.";
        if (!policyId.StartsWith(Prefix, StringComparison.Ordinal))
            return policyId.Replace('.', '/');

        var trimmed = policyId[Prefix.Length..];

        // Drop the trailing action token (everything after the last dot).
        var lastDot = trimmed.LastIndexOf('.');
        var packagePart = lastDot > 0 ? trimmed[..lastDot] : trimmed;

        return packagePart.Replace('.', '/');
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
