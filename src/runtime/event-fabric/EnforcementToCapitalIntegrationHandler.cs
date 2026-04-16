using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Enforcement -> Capital event-driven adapter. Subscribes to the violation
/// event stream and, for every <see cref="ViolationDetectedEventSchema"/>
/// whose RecommendedAction is a hard Block, dispatches a
/// <see cref="FreezeCapitalAccountCommand"/> against the capital account
/// identified by SourceReference. Softer actions (Warn / Restrict / Escalate)
/// are handled on the dispatch hot path by ExecutionGuardMiddleware via
/// IViolationStateQuery and require no command here.
///
/// <para>
/// Domain isolation: enforcement never references capital types; capital
/// never references enforcement types. The bridge is realised here as a
/// shared-contract consumer that emits a shared-contract command, routed
/// through ISystemIntentDispatcher and the full runtime pipeline.
/// </para>
///
/// <para>
/// Idempotency: envelope-level claim via <see cref="IIdempotencyStore"/>
/// keyed on <c>enforcement-to-capital:{EventId}</c>. Claim released on
/// dispatch failure so genuine retries remain possible. Dispatcher-level
/// IdempotencyMiddleware + aggregate-level version gating provide the
/// second and third layers.
/// </para>
/// </summary>
public sealed class EnforcementToCapitalIntegrationHandler
{
    private const string IdempotencyKeyPrefix = "enforcement-to-capital";
    private const string BlockAction = "Block";

    private static readonly DomainRoute CapitalAccountRoute =
        new("economic", "capital", "account");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public EnforcementToCapitalIntegrationHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not ViolationDetectedEventSchema violation) return;
        if (!IsDispatchable(violation)) return;

        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            var command = new FreezeCapitalAccountCommand(
                violation.SourceReference,
                $"Enforcement violation {envelope.AggregateId}: {violation.Reason}");

            await _dispatcher.DispatchAsync(command, CapitalAccountRoute, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private static bool IsDispatchable(ViolationDetectedEventSchema violation)
    {
        if (violation.SourceReference == Guid.Empty) return false;
        if (!string.Equals(violation.RecommendedAction, BlockAction, StringComparison.Ordinal)) return false;
        return true;
    }
}
