using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Ledger → Capital event-driven adapter. Subscribes to ledger events
/// and projects each posted journal entry into a capital account
/// mutation, without any direct coupling between the two domains.
///
/// Mapping (standard accounting convention for asset-class capital
/// accounts: Debit increases cash, Credit decreases cash):
///   • Debit  on a capital account → FundCapitalAccountCommand (inflow)
///   • Credit on a capital account → AllocateCapitalAccountCommand (outflow)
///
/// <para>
/// Reserve / ReleaseReservation are intent-level reservations, not
/// ledger postings — they are not produced here.
/// </para>
///
/// <para>
/// Replay safety: two-layer idempotency. (1) Envelope-level claim via
/// <see cref="IIdempotencyStore"/> keyed on
/// <c>ledger-to-capital:{EventId}</c> short-circuits redelivered Kafka
/// messages before dispatch. The claim is released on dispatch failure
/// so a genuine retry remains possible. (2) The downstream dispatcher's
/// <c>IdempotencyMiddleware</c> deduplicates by deterministic CommandId
/// and the capital aggregate's own version gating (per
/// <c>E-VERSION-01</c>) prevents double-apply at the write path.
/// <see cref="LedgerUpdatedEvent"/> is observed as the reconciliation
/// checkpoint; the actionable signal is the per-entry
/// <see cref="JournalEntryRecordedEventSchema"/>.
/// </para>
///
/// <para>
/// Wired-in state: attached to <c>LedgerToCapitalIntegrationWorker</c>
/// via <c>EconomicCompositionRoot</c>. The worker subscribes to
/// <c>whyce.economic.ledger.journal.events</c> and
/// <c>whyce.economic.ledger.ledger.events</c> under a dedicated
/// consumer group so malformed-message handling is isolated from the
/// projection pipeline.
/// </para>
/// </summary>
public sealed class LedgerToCapitalIntegrationHandler
{
    private const string BookingDirectionDebit = "Debit";
    private const string BookingDirectionCredit = "Credit";
    private const string IdempotencyKeyPrefix = "ledger-to-capital";

    private static readonly DomainRoute CapitalAccountRoute =
        new("economic", "capital", "account");

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdempotencyStore _idempotencyStore;

    public LedgerToCapitalIntegrationHandler(
        ISystemIntentDispatcher dispatcher,
        IIdempotencyStore idempotencyStore)
    {
        _dispatcher = dispatcher;
        _idempotencyStore = idempotencyStore;
    }

    public async Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        if (envelope.Payload is not JournalEntryRecordedEventSchema entry)
        {
            // LedgerUpdatedEventSchema and all other payloads are observation-only.
            // Per-entry capital mutation is driven exclusively by JournalEntryRecordedEvent.
            return;
        }

        if (!IsDispatchable(entry)) return;

        var idempotencyKey = $"{IdempotencyKeyPrefix}:{envelope.EventId}";
        var claimed = await _idempotencyStore.TryClaimAsync(idempotencyKey, cancellationToken);
        if (!claimed) return;

        try
        {
            await DispatchCapitalMutation(entry, cancellationToken);
        }
        catch
        {
            await _idempotencyStore.ReleaseAsync(idempotencyKey, cancellationToken);
            throw;
        }
    }

    private static bool IsDispatchable(JournalEntryRecordedEventSchema entry)
    {
        if (entry.AccountId == Guid.Empty) return false;
        if (entry.Amount <= 0m) return false;
        if (string.IsNullOrWhiteSpace(entry.Currency)) return false;
        return entry.Direction is BookingDirectionDebit or BookingDirectionCredit;
    }

    private Task DispatchCapitalMutation(JournalEntryRecordedEventSchema entry, CancellationToken ct)
    {
        object command = entry.Direction switch
        {
            BookingDirectionDebit =>
                new FundCapitalAccountCommand(entry.AccountId, entry.Amount, entry.Currency),
            BookingDirectionCredit =>
                new AllocateCapitalAccountCommand(entry.AccountId, entry.Amount, entry.Currency),
            _ => throw new InvalidOperationException(
                $"Unreachable: direction '{entry.Direction}' passed IsDispatchable.")
        };

        return _dispatcher.DispatchAsync(command, CapitalAccountRoute, ct);
    }
}
