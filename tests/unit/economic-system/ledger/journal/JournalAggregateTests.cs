using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Journal;

/// <summary>
/// Unit coverage for <see cref="JournalAggregate"/>. Focus areas:
///   • Lifecycle: Create → AddEntry* → Post state machine.
///   • Invariants: amount>0, entryId non-empty, accountId non-empty,
///     currency consistency across entries, ≥2 entries required to post,
///     debit-sum == credit-sum (core double-entry invariant — INV-101).
///   • Terminal state: Posted rejects further mutation.
///   • Replay: event history rehydrates equivalent state.
/// Every test exercises a distinct rule; no duplicate coverage.
/// </summary>
public sealed class JournalAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp CreatedAt = new(new DateTimeOffset(2026, 4, 16, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp PostedAt  = new(new DateTimeOffset(2026, 4, 16, 12, 5, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");
    private static readonly Currency Eur = new("EUR");

    private static JournalAggregate NewJournal(string seed)
    {
        var journalId = new JournalId(IdGen.Generate($"JournalAggregateTests:{seed}:journal"));
        return JournalAggregate.Create(journalId, CreatedAt);
    }

    // ── Lifecycle ────────────────────────────────────────────────

    [Fact]
    public void Create_RaisesJournalCreatedEvent_EntersOpenStatus()
    {
        var journal = NewJournal("Create");

        var evt = Assert.IsType<JournalCreatedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Equal(journal.JournalId, evt.JournalId);
        Assert.Equal(CreatedAt,          evt.CreatedAt);
        Assert.Equal(JournalStatus.Open, journal.Status);
        Assert.Empty(journal.Entries);
    }

    [Fact]
    public void AddEntry_Single_UpdatesCurrencyAndAppendsEntry()
    {
        var journal = NewJournal("AddEntry_Single");
        journal.ClearDomainEvents();
        var entryId   = IdGen.Generate("AddEntry_Single:entry");
        var accountId = IdGen.Generate("AddEntry_Single:account");

        journal.AddEntry(entryId, accountId, new Amount(100m), Usd, BookingDirection.Debit);

        var evt = Assert.IsType<JournalEntryAddedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Equal(entryId,                evt.EntryId);
        Assert.Equal(accountId,              evt.AccountId);
        Assert.Equal(100m,                   evt.Amount.Value);
        Assert.Equal("USD",                  evt.Currency.Code);
        Assert.Equal(BookingDirection.Debit, evt.Direction);
        Assert.Single(journal.Entries);
        Assert.Equal("USD", journal.Currency.Code);
    }

    [Fact]
    public void Post_BalancedDebitCredit_RaisesJournalPostedEvent_MovesToPosted()
    {
        var journal = NewJournal("Post_Balanced");
        journal.AddEntry(IdGen.Generate("Post_Balanced:debit"),  IdGen.Generate("Post_Balanced:acc1"), new Amount(250m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("Post_Balanced:credit"), IdGen.Generate("Post_Balanced:acc2"), new Amount(250m), Usd, BookingDirection.Credit);
        journal.ClearDomainEvents();

        journal.Post(PostedAt);

        var evt = Assert.IsType<JournalPostedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Equal(250m, evt.TotalDebit.Value);
        Assert.Equal(250m, evt.TotalCredit.Value);
        Assert.Equal(PostedAt, evt.PostedAt);
        Assert.Equal(JournalStatus.Posted, journal.Status);
        Assert.Equal(PostedAt, journal.PostedAt);
    }

    // ── Core Invariants ─────────────────────────────────────────

    [Fact]
    public void Post_DebitNotEqualCredit_Throws_UnbalancedInvariant()
    {
        var journal = NewJournal("Post_Unbalanced");
        journal.AddEntry(IdGen.Generate("Post_Unbalanced:debit"),  IdGen.Generate("Post_Unbalanced:acc1"), new Amount(100m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("Post_Unbalanced:credit"), IdGen.Generate("Post_Unbalanced:acc2"), new Amount( 75m), Usd, BookingDirection.Credit);

        var ex = Assert.ThrowsAny<Exception>(() => journal.Post(PostedAt));
        Assert.Contains("balanc", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(JournalStatus.Open, journal.Status);
    }

    [Fact]
    public void Post_FewerThanTwoEntries_Throws()
    {
        var journal = NewJournal("Post_Empty");
        Assert.ThrowsAny<Exception>(() => journal.Post(PostedAt));

        var journalOne = NewJournal("Post_OneEntry");
        journalOne.AddEntry(IdGen.Generate("Post_OneEntry:debit"), IdGen.Generate("Post_OneEntry:acc"), new Amount(10m), Usd, BookingDirection.Debit);
        Assert.ThrowsAny<Exception>(() => journalOne.Post(PostedAt));
    }

    [Fact]
    public void AddEntry_ZeroOrNegativeAmount_Throws()
    {
        var journal = NewJournal("AddEntry_Bad");
        var entryId = IdGen.Generate("AddEntry_Bad:entry");
        var accId   = IdGen.Generate("AddEntry_Bad:acc");

        Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(entryId, accId, new Amount(0m), Usd, BookingDirection.Debit));
        Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(entryId, accId, new Amount(-5m), Usd, BookingDirection.Credit));
    }

    [Fact]
    public void AddEntry_MissingEntryId_Throws()
    {
        var journal = NewJournal("AddEntry_MissingEntry");
        Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(Guid.Empty, IdGen.Generate("acc"), new Amount(1m), Usd, BookingDirection.Debit));
    }

    [Fact]
    public void AddEntry_MissingAccountId_Throws()
    {
        var journal = NewJournal("AddEntry_MissingAcc");
        Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(IdGen.Generate("e"), Guid.Empty, new Amount(1m), Usd, BookingDirection.Debit));
    }

    [Fact]
    public void AddEntry_CurrencyMismatch_AfterFirstEntry_Throws()
    {
        var journal = NewJournal("AddEntry_CurrencyMismatch");
        journal.AddEntry(IdGen.Generate("e1"), IdGen.Generate("a1"), new Amount(10m), Usd, BookingDirection.Debit);

        var ex = Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(IdGen.Generate("e2"), IdGen.Generate("a2"), new Amount(10m), Eur, BookingDirection.Credit));
        Assert.Contains("USD", ex.Message);
    }

    [Fact]
    public void AddEntry_OnPostedJournal_Throws()
    {
        var journal = NewJournal("AddEntry_AfterPost");
        journal.AddEntry(IdGen.Generate("a"), IdGen.Generate("acc1"), new Amount(1m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("b"), IdGen.Generate("acc2"), new Amount(1m), Usd, BookingDirection.Credit);
        journal.Post(PostedAt);

        Assert.ThrowsAny<Exception>(() =>
            journal.AddEntry(IdGen.Generate("c"), IdGen.Generate("acc3"), new Amount(1m), Usd, BookingDirection.Debit));
    }

    [Fact]
    public void Post_TwicePosted_Throws()
    {
        var journal = NewJournal("Post_Twice");
        journal.AddEntry(IdGen.Generate("d"), IdGen.Generate("acc1"), new Amount(5m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("c"), IdGen.Generate("acc2"), new Amount(5m), Usd, BookingDirection.Credit);
        journal.Post(PostedAt);

        Assert.ThrowsAny<Exception>(() => journal.Post(PostedAt));
    }

    // ── Replay ──────────────────────────────────────────────────

    [Fact]
    public void LoadFromHistory_RehydratesStateAndInvariant()
    {
        var journalId = new JournalId(IdGen.Generate("History:journal"));
        var e1 = IdGen.Generate("History:e1"); var a1 = IdGen.Generate("History:a1");
        var e2 = IdGen.Generate("History:e2"); var a2 = IdGen.Generate("History:a2");

        var history = new object[]
        {
            new JournalCreatedEvent(journalId, CreatedAt),
            new JournalEntryAddedEvent(journalId, e1, a1, new Amount(42m), Usd, BookingDirection.Debit),
            new JournalEntryAddedEvent(journalId, e2, a2, new Amount(42m), Usd, BookingDirection.Credit),
            new JournalPostedEvent(journalId, new Amount(42m), new Amount(42m), PostedAt)
        };

        var aggregate = (JournalAggregate)Activator.CreateInstance(typeof(JournalAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(journalId, aggregate.JournalId);
        Assert.Equal("USD",      aggregate.Currency.Code);
        Assert.Equal(JournalStatus.Posted, aggregate.Status);
        Assert.Equal(PostedAt,   aggregate.PostedAt);
        Assert.Equal(2,          aggregate.Entries.Count);
        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void MultipleEntries_BalanceSumsCorrectly_AcrossManyEntries()
    {
        var journal = NewJournal("MultiEntry_Balanced");
        journal.AddEntry(IdGen.Generate("m:d1"), IdGen.Generate("m:a1"), new Amount(100m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("m:d2"), IdGen.Generate("m:a2"), new Amount( 50m), Usd, BookingDirection.Debit);
        journal.AddEntry(IdGen.Generate("m:c1"), IdGen.Generate("m:a3"), new Amount( 80m), Usd, BookingDirection.Credit);
        journal.AddEntry(IdGen.Generate("m:c2"), IdGen.Generate("m:a4"), new Amount( 70m), Usd, BookingDirection.Credit);
        journal.ClearDomainEvents();

        journal.Post(PostedAt);

        var evt = Assert.IsType<JournalPostedEvent>(Assert.Single(journal.DomainEvents));
        Assert.Equal(150m, evt.TotalDebit.Value);
        Assert.Equal(150m, evt.TotalCredit.Value);
    }
}
