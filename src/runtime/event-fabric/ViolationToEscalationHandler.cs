using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Violation → Escalation event-driven adapter. Subscribes to the violation
/// event stream and dispatches one <see cref="AccumulateViolationCommand"/>
/// per <see cref="ViolationDetectedEventSchema"/>, routed to the escalation
/// aggregate keyed on the violation's SourceReference (the subject).
///
/// Replay safety: envelope-level idempotency claim on
/// <c>violation-to-escalation:{EventId}</c>; the aggregate itself is
/// version-gated so redelivery beyond the claim is idempotent.
/// </summary>
public sealed class ViolationToEscalationHandler
{
    private const string IdempotencyKeyPrefix = "violation-to-escalation";

    private static readonly DomainRoute EscalationRoute =
        new("economic", "enforcement", "escalation");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public ViolationToEscalationHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not ViolationDetectedEventSchema violation) return;
        if (violation.SourceReference == Guid.Empty) return;

        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var command = new AccumulateViolationCommand(
                violation.SourceReference,
                violation.AggregateId,
                violation.Severity,
                violation.DetectedAt);

            await _dispatcher.DispatchAsync(command, EscalationRoute, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }
}
