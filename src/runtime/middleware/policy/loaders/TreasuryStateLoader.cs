using Whycespace.Domain.EconomicSystem.Ledger.Treasury;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Runtime.Middleware.Policy.Loaders;

/// <summary>
/// Phase 11 B2 — <see cref="IAggregateStateLoader"/> for the treasury
/// aggregate. Shape parity with <see cref="ObligationStateLoader"/>: replay
/// events from <see cref="IEventStore"/> through the REAL
/// <c>TreasuryAggregate.Apply</c> reducer, project into
/// <see cref="TreasuryStateSnapshot"/>.
///
/// <para>
/// <b>Command scope.</b> Three treasury-aggregate-bound commands trigger a
/// load: <see cref="CreateTreasuryCommand"/>, <see cref="AllocateFundsCommand"/>,
/// <see cref="ReleaseFundsCommand"/>. Any other command type returns
/// <c>null</c>.
/// </para>
///
/// <para>
/// <b>Empty-stream contract.</b> Returns <c>null</c> for factory commands
/// (treasury does not exist yet). Rego's
/// <c>allocation_balance_ok if { not input.resource.state }</c> fallback
/// handles the null case — no behavioural regression.
/// </para>
/// </summary>
public sealed class TreasuryStateLoader : IAggregateStateLoader
{
    private static readonly HashSet<Type> HandledCommands = new()
    {
        typeof(CreateTreasuryCommand),
        typeof(AllocateFundsCommand),
        typeof(ReleaseFundsCommand),
    };

    public static bool Handles(Type commandType) => HandledCommands.Contains(commandType);

    private readonly IEventStore _eventStore;

    public TreasuryStateLoader(IEventStore eventStore)
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

        var aggregate = (TreasuryAggregate)Activator.CreateInstance(
            typeof(TreasuryAggregate), nonPublic: true)!;
        aggregate.HydrateIdentity(aggregateId);
        aggregate.LoadFromHistory(events);

        return new TreasuryStateSnapshot(
            Balance: aggregate.Balance.Value,
            Currency: aggregate.Currency.Code);
    }
}
