using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Journal;

/// <summary>
/// Phase 6 T6.1 — pins the FX-rate-snapshot carriage on journal entries.
/// Ledger read path must never re-resolve exchange state: the (RateId,
/// Rate) tuple from FxLockStep is stamped onto every entry at post time
/// and replayed verbatim.
/// </summary>
public sealed class JournalEntryFxSnapshotTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FrozenNow = new(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

    private static JournalAggregate NewJournal(string seed)
    {
        var journalId = new JournalId(IdGen.Generate($"JournalEntryFxSnapshotTests:{seed}:journal"));
        return JournalAggregate.Create(journalId, FrozenNow);
    }

    [Fact]
    public void AddEntry_WithoutFxSnapshot_AppliesEntryAndLeavesRateFieldsNull()
    {
        var journal = NewJournal("NoFx");
        var entryId = IdGen.Generate("NoFx:entry");
        var accountId = IdGen.Generate("NoFx:account");
        journal.ClearDomainEvents();

        journal.AddEntry(entryId, accountId, new Amount(100m), new Currency("USD"), BookingDirection.Debit);

        var evt = Assert.IsType<JournalEntryAddedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Null(evt.FxRateId);
        Assert.Null(evt.FxRate);
    }

    [Fact]
    public void AddEntry_WithFxSnapshot_StampsRateOntoEvent()
    {
        var journal = NewJournal("WithFx");
        var entryId = IdGen.Generate("WithFx:entry");
        var accountId = IdGen.Generate("WithFx:account");
        var rateId = IdGen.Generate("WithFx:rate");
        journal.ClearDomainEvents();

        journal.AddEntry(
            entryId, accountId,
            new Amount(100m), new Currency("USD"),
            BookingDirection.Debit,
            fxRateId: rateId, fxRate: 1.25m);

        var evt = Assert.IsType<JournalEntryAddedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Equal(rateId, evt.FxRateId);
        Assert.Equal(1.25m, evt.FxRate);
    }

    [Fact]
    public void AddEntry_WithPartialFxSnapshot_Throws()
    {
        var journal = NewJournal("PartialFx");
        var entryId = IdGen.Generate("PartialFx:entry");
        var accountId = IdGen.Generate("PartialFx:account");
        var rateId = IdGen.Generate("PartialFx:rate");

        Assert.ThrowsAny<DomainException>(() =>
            journal.AddEntry(
                entryId, accountId,
                new Amount(100m), new Currency("USD"),
                BookingDirection.Debit,
                fxRateId: rateId, fxRate: null));

        Assert.ThrowsAny<DomainException>(() =>
            journal.AddEntry(
                entryId, accountId,
                new Amount(100m), new Currency("USD"),
                BookingDirection.Debit,
                fxRateId: null, fxRate: 1.25m));
    }

    [Fact]
    public void AddEntry_WithNonPositiveFxRate_Throws()
    {
        var journal = NewJournal("BadRate");
        var entryId = IdGen.Generate("BadRate:entry");
        var accountId = IdGen.Generate("BadRate:account");
        var rateId = IdGen.Generate("BadRate:rate");

        Assert.ThrowsAny<DomainException>(() =>
            journal.AddEntry(
                entryId, accountId,
                new Amount(100m), new Currency("USD"),
                BookingDirection.Debit,
                fxRateId: rateId, fxRate: 0m));
    }

    [Fact]
    public void LoadFromHistory_WithFxSnapshotEvent_ReconstitutesEntryWithRate()
    {
        var journalId = new JournalId(IdGen.Generate("LoadFx:journal"));
        var entryId = IdGen.Generate("LoadFx:entry");
        var accountId = IdGen.Generate("LoadFx:account");
        var rateId = IdGen.Generate("LoadFx:rate");

        var history = new object[]
        {
            new JournalCreatedEvent(journalId, FrozenNow),
            new JournalEntryAddedEvent(
                journalId, entryId, accountId,
                new Amount(100m), new Currency("USD"),
                BookingDirection.Debit,
                rateId, 1.25m),
        };

        var aggregate = (JournalAggregate)Activator.CreateInstance(typeof(JournalAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        var entry = Assert.Single(aggregate.Entries);
        Assert.Equal(rateId, entry.FxRateId);
        Assert.Equal(1.25m, entry.FxRate);
    }
}
