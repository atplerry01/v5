using Whycespace.Domain.EconomicSystem.Ledger.Entry;
using Whycespace.Domain.EconomicSystem.Ledger.Journal;
using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Ledger.Entry;

public sealed class LedgerEntryAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp BaseTime = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Currency Usd = new("USD");

    private static EntryId NewEntryId(string seed) =>
        new(IdGen.Generate($"LedgerEntryAggregateTests:{seed}:entry"));

    [Fact]
    public void Record_RaisesLedgerEntryRecordedEvent()
    {
        var id = NewEntryId("Record_Valid");
        var journalId = new JournalId(IdGen.Generate("LedgerEntryAggregateTests:Record_Valid:journal"));
        var accountId = new AccountId(IdGen.Generate("LedgerEntryAggregateTests:Record_Valid:account"));

        var aggregate = LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(500m), Usd, EntryDirection.Debit, BaseTime);

        var evt = Assert.IsType<LedgerEntryRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.EntryId);
        Assert.Equal(500m, evt.Amount.Value);
        Assert.Equal(EntryDirection.Debit, evt.Direction);
    }

    [Fact]
    public void Record_SetsStateFromEvent()
    {
        var id = NewEntryId("Record_State");
        var journalId = new JournalId(IdGen.Generate("LedgerEntryAggregateTests:Record_State:journal"));
        var accountId = new AccountId(IdGen.Generate("LedgerEntryAggregateTests:Record_State:account"));

        var aggregate = LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(250m), Usd, EntryDirection.Credit, BaseTime);

        Assert.Equal(id, aggregate.EntryId);
        Assert.Equal(250m, aggregate.Amount.Value);
        Assert.Equal(EntryDirection.Credit, aggregate.Direction);
        Assert.True(aggregate.IsRecorded);
    }

    [Fact]
    public void Record_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewEntryId("Stable");
        var journalId = new JournalId(IdGen.Generate("LedgerEntryAggregateTests:Stable:journal"));
        var accountId = new AccountId(IdGen.Generate("LedgerEntryAggregateTests:Stable:account"));

        var a1 = LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(100m), Usd, EntryDirection.Debit, BaseTime);
        var a2 = LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(100m), Usd, EntryDirection.Debit, BaseTime);

        Assert.Equal(
            ((LedgerEntryRecordedEvent)a1.DomainEvents[0]).EntryId.Value,
            ((LedgerEntryRecordedEvent)a2.DomainEvents[0]).EntryId.Value);
    }

    [Fact]
    public void Record_ZeroAmount_Throws()
    {
        var id = NewEntryId("Zero");
        var journalId = new JournalId(IdGen.Generate("LedgerEntryAggregateTests:Zero:journal"));
        var accountId = new AccountId(IdGen.Generate("LedgerEntryAggregateTests:Zero:account"));

        Assert.ThrowsAny<Exception>(() =>
            LedgerEntryAggregate.Record(id, journalId, accountId, new Amount(0m), Usd, EntryDirection.Debit, BaseTime));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewEntryId("History");
        var journalId = new JournalId(IdGen.Generate("LedgerEntryAggregateTests:History:journal"));
        var accountId = new AccountId(IdGen.Generate("LedgerEntryAggregateTests:History:account"));

        var history = new object[]
        {
            new LedgerEntryRecordedEvent(id, journalId.Value, accountId.Value, new Amount(750m), Usd, EntryDirection.Credit, BaseTime)
        };

        var aggregate = (LedgerEntryAggregate)Activator.CreateInstance(typeof(LedgerEntryAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.EntryId);
        Assert.Equal(750m, aggregate.Amount.Value);
        Assert.Equal(EntryDirection.Credit, aggregate.Direction);
        Assert.True(aggregate.IsRecorded);
        Assert.Empty(aggregate.DomainEvents);
    }
}
