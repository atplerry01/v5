using Microsoft.Extensions.Logging;
using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Phase 6 T6.5 — risk → enforcement bridge. Consumes
/// <c>ExposureBreachedEvent</c> envelopes from the economic risk
/// exposure topic and dispatches a <see cref="DetectViolationCommand"/>
/// into the enforcement pipeline. This closes the loop deterministically
/// without routing through OPA rego: every breach event produces exactly
/// one violation detection.
///
/// Replay safety: the synthesised <c>ViolationId</c> is derived from the
/// source envelope's <c>EventId</c>, so re-delivery of the same breach
/// event yields the same violation id and the downstream engine's
/// idempotency middleware dedupes at the write path.
/// </summary>
public sealed class RiskExposureEnforcementHandler
{
    private const string BreachEventType = "ExposureBreachedEvent";
    private const string DefaultSeverity = "Critical";
    private const string DefaultRecommendedAction = "Restrict";

    private static readonly DomainRoute ViolationRoute =
        new("economic", "enforcement", "violation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly ILogger<RiskExposureEnforcementHandler>? _logger;

    public RiskExposureEnforcementHandler(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IClock clock,
        ILogger<RiskExposureEnforcementHandler>? logger = null)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _clock = clock;
        _logger = logger;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.EventType != BreachEventType) return;

        var violationId = _idGenerator.Generate(
            $"risk-breach-violation:{envelope.EventId}");

        // RuleId derived from the exposure aggregate so every breach on
        // the same exposure reuses the same rule reference for grouping.
        var ruleId = _idGenerator.Generate(
            $"risk-breach-rule:{envelope.AggregateId}");

        var command = new DetectViolationCommand(
            ViolationId: violationId,
            RuleId: ruleId,
            SourceReference: envelope.AggregateId,
            Reason: $"Risk exposure breached threshold (ExposureId={envelope.AggregateId}).",
            Severity: DefaultSeverity,
            RecommendedAction: DefaultRecommendedAction,
            DetectedAt: _clock.UtcNow);

        var result = await _dispatcher.DispatchAsync(command, ViolationRoute, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger?.LogError(
                "Risk breach → enforcement dispatch failed. ExposureId={ExposureId}, Error={Error}",
                envelope.AggregateId, result.Error);
            throw new InvalidOperationException(
                $"DetectViolationCommand dispatch failed for ExposureId={envelope.AggregateId}: {result.Error}");
        }
    }
}
