using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Engines.T2E.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.EconomicSystem.Ledger.Journal;

/// <summary>
/// Phase 4.5 T4.5.4 — proves the engine-level control-plane gate on the
/// ledger boundary:
///   * IsSystem == false → handler throws ControlPlaneOriginRequired and
///     emits no events. Closes the JournalController bypass (Phase 4
///     Finding 12) at the engine boundary, regardless of API-side auth.
///   * IsSystem == true (the path taken by both the transaction
///     PostToLedgerStep and the payout PostLedgerJournalStep via
///     DispatchSystemAsync) → handler executes normally and emits the
///     full journal + ledger event chain.
/// </summary>
public sealed class PostJournalEntriesControlPlaneOriginTests
{
    private sealed class StubEngineContext : IEngineContext
    {
        private readonly List<object> _emitted = new();
        private readonly object? _preloaded;

        public StubEngineContext(object command, bool isSystem, object? preloaded = null)
        {
            Command = command;
            AggregateId = command is IHasAggregateId h ? h.AggregateId : Guid.Empty;
            EnforcementConstraint = null;
            IsSystem = isSystem;
            _preloaded = preloaded;
        }

        public object Command { get; }
        public Guid AggregateId { get; }
        public string? EnforcementConstraint { get; }
        public bool IsSystem { get; }
        public IReadOnlyList<object> EmittedEvents => _emitted;

        public Task<object> LoadAggregateAsync(Type aggregateType) =>
            _preloaded is null
                ? throw new InvalidOperationException("No preloaded aggregate.")
                : Task.FromResult(_preloaded);

        public void EmitEvents(IReadOnlyList<object> events) => _emitted.AddRange(events);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow { get; }
        public FixedClock(DateTimeOffset now) => UtcNow = now;
    }

    private static readonly DateTimeOffset Now =
        new(2026, 4, 17, 12, 30, 0, TimeSpan.Zero);

    private static PostJournalEntriesCommand BalancedDebitCreditCommand(out Guid ledgerId)
    {
        ledgerId = Guid.NewGuid();
        return new PostJournalEntriesCommand(
            ledgerId,
            JournalId: Guid.NewGuid(),
            Entries: new List<JournalEntryInput>
            {
                new(Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", "Debit"),
                new(Guid.NewGuid(), Guid.NewGuid(), 100m, "USD", "Credit"),
            });
    }

    private static LedgerAggregate FreshLedger(Guid ledgerId)
    {
        var aggregate = (LedgerAggregate)Activator.CreateInstance(typeof(LedgerAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(new object[]
        {
            new LedgerOpenedEvent(new LedgerId(ledgerId), new Currency("USD"), new Timestamp(Now))
        });
        return aggregate;
    }

    [Fact]
    public async Task DirectApiPath_IsSystemFalse_ThrowsControlPlaneOriginRequired_AndEmitsNothing()
    {
        // Simulates the JournalController hitting the handler via DispatchAsync.
        // The handler must hard-reject and emit no events so no ledger
        // mutation persists from the bypassed path.
        var cmd = BalancedDebitCreditCommand(out var ledgerId);
        var ledger = FreshLedger(ledgerId);
        var ctx = new StubEngineContext(cmd, isSystem: false, preloaded: ledger);
        var handler = new PostJournalEntriesHandler(new FixedClock(Now));

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.ExecuteAsync(ctx));
        Assert.Contains("control plane", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(ctx.EmittedEvents);
    }

    [Fact]
    public async Task SystemOriginPath_FromTransactionLifecycleOrPayout_IsAccepted()
    {
        // Simulates either PostToLedgerStep or PostLedgerJournalStep —
        // both use DispatchSystemAsync, which sets IsSystem=true. The
        // handler executes the full journal + entry + ledger emission.
        var cmd = BalancedDebitCreditCommand(out var ledgerId);
        var ledger = FreshLedger(ledgerId);
        var ctx = new StubEngineContext(cmd, isSystem: true, preloaded: ledger);
        var handler = new PostJournalEntriesHandler(new FixedClock(Now));

        await handler.ExecuteAsync(ctx);

        Assert.NotEmpty(ctx.EmittedEvents);
        Assert.Contains(ctx.EmittedEvents, e => e is JournalCreatedEvent);
        Assert.Contains(ctx.EmittedEvents, e => e is JournalPostedEvent);
        Assert.Contains(ctx.EmittedEvents, e => e is JournalAppendedToLedgerEvent);
    }

    [Fact]
    public async Task DirectApiPath_DoesNotMutateLedgerAggregate()
    {
        // Defense-in-depth assertion: even though the handler throws,
        // verify the ledger aggregate's domain-events buffer remains
        // empty after the rejected call, so an enclosing transaction
        // would have nothing to persist.
        var cmd = BalancedDebitCreditCommand(out var ledgerId);
        var ledger = FreshLedger(ledgerId);
        ledger.ClearDomainEvents();

        var ctx = new StubEngineContext(cmd, isSystem: false, preloaded: ledger);
        var handler = new PostJournalEntriesHandler(new FixedClock(Now));

        await Assert.ThrowsAsync<DomainException>(() => handler.ExecuteAsync(ctx));

        Assert.Empty(ledger.DomainEvents);
        Assert.Empty(ctx.EmittedEvents);
    }
}
