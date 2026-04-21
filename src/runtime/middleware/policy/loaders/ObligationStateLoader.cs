using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Shared.Contracts.Economic.Ledger.Obligation;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Runtime.Middleware.Policy.Loaders;

/// <summary>
/// Phase 11 B1 — <see cref="IAggregateStateLoader"/> for the obligation
/// aggregate. Hydrates the policy-visible snapshot by replaying events
/// from <see cref="IEventStore"/> through the REAL
/// <c>ObligationAggregate.Apply</c> reducer — the aggregate itself is the
/// single source of truth for state reduction. No parallel reducer, no
/// drift risk between this loader and the engine's own replay path
/// (<c>RuntimeCommandDispatcher</c>'s aggregate-reconstruction loader).
///
/// <para>
/// <b>Event-store fidelity (<c>POLICY-STATE-SOURCE-EVENT-STORE-01</c>).</b>
/// Events source exclusively from <see cref="IEventStore.LoadEventsAsync"/>.
/// Per <c>EVENT-STORE-HOLDS-MAPPED-PAYLOAD-01</c> rule 4, the Postgres
/// read-side deserialises stored JSON back to domain types via
/// <c>EventDeserializer.DeserializeStored</c> using <c>StoredEventType</c>,
/// so the aggregate's <c>Apply</c> pattern-match on domain event types
/// (<c>ObligationCreatedEvent</c>, <c>ObligationFulfilledEvent</c>,
/// <c>ObligationCancelledEvent</c>) resolves correctly in production.
/// </para>
///
/// <para>
/// <b>Command scope.</b> Only the three obligation-aggregate-bound
/// commands trigger a load: <see cref="CreateObligationCommand"/>,
/// <see cref="FulfilObligationCommand"/>, <see cref="CancelObligationCommand"/>.
/// Any other command type returns <c>null</c> — this is both a safety net
/// (the <c>CompositeAggregateStateLoader</c> in B3 should never route an
/// unrelated command here) and a concrete statement of this loader's
/// ownership boundary.
/// </para>
///
/// <para>
/// <b>Missing-aggregate contract.</b> Returns <c>null</c> when the event
/// stream is empty (factory command on a not-yet-existing aggregate).
/// Rego's backward-compat branch
/// (<c>fulfilment_state_ok if { not input.resource.state }</c>) handles
/// the null case cleanly — no behavioural regression for factory-path
/// commands.
/// </para>
/// </summary>
public sealed class ObligationStateLoader : IAggregateStateLoader
{
    private static readonly HashSet<Type> HandledCommands = new()
    {
        typeof(CreateObligationCommand),
        typeof(FulfilObligationCommand),
        typeof(CancelObligationCommand),
    };

    /// <summary>
    /// Exposed so <c>CompositeAggregateStateLoader</c> can do type-based
    /// routing without taking a dependency on this loader's private set.
    /// </summary>
    public static bool Handles(Type commandType) => HandledCommands.Contains(commandType);

    private readonly IEventStore _eventStore;

    public ObligationStateLoader(IEventStore eventStore)
    {
        ArgumentNullException.ThrowIfNull(eventStore);
        _eventStore = eventStore;
    }

    public async Task<object?> LoadSnapshotAsync(
        Type commandType,
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        if (!HandledCommands.Contains(commandType)) return null;
        if (aggregateId == Guid.Empty) return null;

        var events = await _eventStore.LoadEventsAsync(aggregateId, cancellationToken);
        if (events.Count == 0) return null;

        var aggregate = (ObligationAggregate)Activator.CreateInstance(
            typeof(ObligationAggregate), nonPublic: true)!;
        aggregate.HydrateIdentity(aggregateId);
        aggregate.LoadFromHistory(events);

        return new ObligationStateSnapshot(
            Status: aggregate.Status.ToString(),
            Amount: aggregate.Amount.Value,
            CounterpartyId: aggregate.CounterpartyId.Value,
            Type: aggregate.Type.ToString(),
            Currency: aggregate.Currency.Code);
    }
}
