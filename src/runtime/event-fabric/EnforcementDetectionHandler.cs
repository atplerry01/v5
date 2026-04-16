using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Enforcement;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Cross-domain detection handler. Consumes an inbound event envelope from
/// an observable source topic (ledger, policy, etc.), delegates rule
/// evaluation to <see cref="IEnforcementEventEvaluator"/>, and for each
/// returned signal dispatches a <see cref="DetectViolationCommand"/>
/// through the canonical <see cref="ISystemIntentDispatcher"/>.
///
/// Replay safety: envelope-level idempotency claim keyed on
/// <c>enforcement-detection:{EventId}</c> short-circuits redelivered source
/// messages before evaluation. The claim is released on dispatch failure
/// so a genuine retry remains possible. Each produced DetectViolation
/// command additionally carries a deterministic ViolationId derived from
/// (source event id, rule id, source reference) so the downstream
/// engine's IdempotencyMiddleware dedupes at the write path.
/// </summary>
public sealed class EnforcementDetectionHandler
{
    private const string IdempotencyKeyPrefix = "enforcement-detection";

    private static readonly DomainRoute ViolationRoute =
        new("economic", "enforcement", "violation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEnforcementEventEvaluator _evaluator;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public EnforcementDetectionHandler(
        ISystemIntentDispatcher dispatcher,
        IEnforcementEventEvaluator evaluator,
        IIdempotencyStore idempotencyStore,
        IIdGenerator idGenerator,
        IClock clock)
    {
        _dispatcher = dispatcher;
        _evaluator = evaluator;
        _idempotencyStore = idempotencyStore;
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var signals = await _evaluator.EvaluateAsync(envelope, cancellationToken);
            if (signals.Count == 0) return;

            foreach (var signal in signals)
            {
                var violationId = _idGenerator.Generate(
                    $"enforcement-violation:{envelope.EventId}:{signal.RuleId}:{signal.SourceReference}");

                var command = new DetectViolationCommand(
                    ViolationId: violationId,
                    RuleId: signal.RuleId,
                    SourceReference: signal.SourceReference,
                    Reason: signal.Reason,
                    Severity: signal.Severity,
                    RecommendedAction: signal.RecommendedAction,
                    DetectedAt: _clock.UtcNow);

                await _dispatcher.DispatchAsync(command, ViolationRoute, cancellationToken);
            }
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }
}
