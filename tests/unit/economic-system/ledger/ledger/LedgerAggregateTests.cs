using Whycespace.Domain.EconomicSystem.Ledger.Ledger;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Ledger;

public sealed class LedgerAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp OpenedAt   = new(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp AppendedAt = new(new DateTimeOffset(2026, 4, 16, 12, 5, 0, TimeSpan.Zero));

    private static LedgerAggregate NewLedger(string seed, string currency = "USD")
    {
        var ledgerId = new LedgerId(IdGen.Generate($"LedgerAggregateTests:{seed}:ledger"));
        return LedgerAggregate.Open(ledgerId, new Currency(currency), OpenedAt);
    }

    [Fact]
    public void Open_RaisesLedgerOpenedEvent()
    {
        var ledger = NewLedger("Open_Valid");

        var evt = Assert.IsType<LedgerOpenedEvent>(Assert.Single(ledger.DomainEvents));
        Assert.Equal(ledger.LedgerId, evt.LedgerId);
        Assert.Equal("USD", evt.Currency.Code);
        Assert.Equal(OpenedAt, evt.OpenedAt);
    }

    [Fact]
    public void AppendJournal_RaisesAppendedAndUpdatedEvents()
    {
        var ledger = NewLedger("Append_Valid");
        ledger.ClearDomainEvents();
        var journalId = IdGen.Generate("LedgerAggregateTests:Append_Valid:journal");

        ledger.AppendJournal(journalId, AppendedAt);

        Assert.Equal(2, ledger.DomainEvents.Count);
        var appended = Assert.IsType<JournalAppendedToLedgerEvent>(ledger.DomainEvents[0]);
        Assert.Equal(ledger.LedgerId, appended.LedgerId);
        Assert.Equal(journalId, appended.JournalId);
        Assert.Equal(AppendedAt, appended.AppendedAt);

        var updated = Assert.IsType<LedgerUpdatedEvent>(ledger.DomainEvents[1]);
        Assert.Equal(ledger.LedgerId, updated.LedgerId);
        Assert.Equal(journalId, updated.JournalId);
        Assert.Equal(1, updated.JournalCount);
    }

    [Fact]
    public void AppendJournal_Duplicate_Throws()
    {
        var ledger = NewLedger("Append_Duplicate");
        var journalId = IdGen.Generate("LedgerAggregateTests:Append_Duplicate:journal");
        ledger.AppendJournal(journalId, AppendedAt);

        var ex = Assert.ThrowsAny<DomainException>(() =>
            ledger.AppendJournal(journalId, AppendedAt));
        Assert.Contains(journalId.ToString(), ex.Message);
    }

    [Fact]
    public void AppendJournal_EmptyJournalId_Throws()
    {
        var ledger = NewLedger("Append_Empty");

        Assert.ThrowsAny<Exception>(() =>
            ledger.AppendJournal(Guid.Empty, AppendedAt));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var ledgerId = new LedgerId(IdGen.Generate("LedgerAggregateTests:History:ledger"));
        var journalId = IdGen.Generate("LedgerAggregateTests:History:journal");

        var history = new object[]
        {
            new LedgerOpenedEvent(ledgerId, new Currency("USD"), OpenedAt),
            new JournalAppendedToLedgerEvent(ledgerId, journalId, AppendedAt),
            new LedgerUpdatedEvent(ledgerId, journalId, 1, AppendedAt)
        };

        var aggregate = (LedgerAggregate)Activator.CreateInstance(typeof(LedgerAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(ledgerId, aggregate.LedgerId);
        Assert.Equal("USD", aggregate.Currency.Code);
        Assert.Equal(OpenedAt, aggregate.OpenedAt);
        Assert.Single(aggregate.Journals);
        Assert.Equal(journalId, aggregate.Journals[0].JournalId);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void Open_WithSameSeed_ProducesStableIdentity()
    {
        var id1 = IdGen.Generate("Open:Stable:seed");
        var id2 = IdGen.Generate("Open:Stable:seed");

        Assert.Equal(id1, id2);

        var l1 = LedgerAggregate.Open(new LedgerId(id1), new Currency("USD"), OpenedAt);
        var l2 = LedgerAggregate.Open(new LedgerId(id2), new Currency("USD"), OpenedAt);

        var e1 = (LedgerOpenedEvent)l1.DomainEvents[0];
        var e2 = (LedgerOpenedEvent)l2.DomainEvents[0];

        Assert.Equal(e1.LedgerId.Value, e2.LedgerId.Value);
        Assert.Equal(e1.Currency.Code, e2.Currency.Code);
        Assert.Equal(e1.OpenedAt, e2.OpenedAt);
    }

    [Fact]
    public void AppendJournal_MultipleJournals_PreservesOrderAndCount()
    {
        var ledger = NewLedger("Append_Many");
        ledger.ClearDomainEvents();

        var j1 = IdGen.Generate("LedgerAggregateTests:Append_Many:j1");
        var j2 = IdGen.Generate("LedgerAggregateTests:Append_Many:j2");
        var j3 = IdGen.Generate("LedgerAggregateTests:Append_Many:j3");

        ledger.AppendJournal(j1, AppendedAt);
        ledger.AppendJournal(j2, AppendedAt);
        ledger.AppendJournal(j3, AppendedAt);

        Assert.Equal(3, ledger.Journals.Count);
        Assert.Equal(j1, ledger.Journals[0].JournalId);
        Assert.Equal(j2, ledger.Journals[1].JournalId);
        Assert.Equal(j3, ledger.Journals[2].JournalId);

        var lastUpdated = ledger.DomainEvents
            .OfType<LedgerUpdatedEvent>()
            .Last();
        Assert.Equal(3, lastUpdated.JournalCount);
    }
}
