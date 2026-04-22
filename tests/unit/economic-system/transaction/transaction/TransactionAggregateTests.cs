using Whycespace.Domain.EconomicSystem.Transaction.Transaction;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Transaction.Transaction;

public sealed class TransactionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp T0 = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T1 = new(new DateTimeOffset(2026, 4, 22, 11, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp T2 = new(new DateTimeOffset(2026, 4, 22, 12, 0, 0, TimeSpan.Zero));

    private static TransactionId NewId(string seed) =>
        new(IdGen.Generate($"TransactionTests:{seed}:tx"));

    private static IReadOnlyList<TransactionReference> OneRef(string seed) =>
        new[] { TransactionReference.Of("expense", IdGen.Generate($"TransactionTests:{seed}:ref")) };

    [Fact]
    public void Initiate_RaisesTransactionInitiatedEvent()
    {
        var id = NewId("Initiate_Valid");
        var refs = OneRef("Initiate_Valid");

        var aggregate = TransactionAggregate.Initiate(id, "transfer", refs, T0);

        var evt = Assert.IsType<TransactionInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.TransactionId);
        Assert.Equal("transfer", evt.Kind);
        Assert.Single(evt.References);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var id = NewId("Initiate_State");

        var aggregate = TransactionAggregate.Initiate(id, "payment", OneRef("Initiate_State"), T0);

        Assert.Equal(TransactionStatus.Initiated, aggregate.Status);
        Assert.Equal(id, aggregate.TransactionId);
    }

    [Fact]
    public void Initiate_WithSameSeed_ProducesStableIdentity()
    {
        var id = NewId("Stable");
        var refs = OneRef("Stable");

        var t1 = TransactionAggregate.Initiate(id, "transfer", refs, T0);
        var t2 = TransactionAggregate.Initiate(id, "transfer", refs, T0);

        Assert.Equal(
            ((TransactionInitiatedEvent)t1.DomainEvents[0]).TransactionId.Value,
            ((TransactionInitiatedEvent)t2.DomainEvents[0]).TransactionId.Value);
    }

    [Fact]
    public void Initiate_EmptyKind_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            TransactionAggregate.Initiate(NewId("EmptyKind"), "", OneRef("EmptyKind"), T0));
    }

    [Fact]
    public void Initiate_EmptyReferences_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            TransactionAggregate.Initiate(NewId("EmptyRefs"), "transfer", Array.Empty<TransactionReference>(), T0));
    }

    [Fact]
    public void MarkProcessing_FromInitiated_RaisesProcessingStartedEvent()
    {
        var id = NewId("Processing");
        var aggregate = TransactionAggregate.Initiate(id, "allocation", OneRef("Processing"), T0);
        aggregate.ClearDomainEvents();

        aggregate.MarkProcessing(T1);

        Assert.IsType<TransactionProcessingStartedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TransactionStatus.Processing, aggregate.Status);
    }

    [Fact]
    public void Commit_FromProcessing_RaisesCommittedEvent()
    {
        var id = NewId("Commit");
        var aggregate = TransactionAggregate.Initiate(id, "transfer", OneRef("Commit"), T0);
        aggregate.MarkProcessing(T1);
        aggregate.ClearDomainEvents();

        aggregate.Commit(T2);

        Assert.IsType<TransactionCommittedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(TransactionStatus.Committed, aggregate.Status);
    }

    [Fact]
    public void Commit_AfterCommit_Throws()
    {
        var id = NewId("Commit_Again");
        var aggregate = TransactionAggregate.Initiate(id, "payment", OneRef("Commit_Again"), T0);
        aggregate.MarkProcessing(T1);
        aggregate.Commit(T2);

        Assert.ThrowsAny<Exception>(() => aggregate.Commit(T2));
    }

    [Fact]
    public void Commit_FromInitiated_Throws()
    {
        var id = NewId("Commit_NotProcessing");
        var aggregate = TransactionAggregate.Initiate(id, "transfer", OneRef("Commit_NotProcessing"), T0);

        Assert.ThrowsAny<Exception>(() => aggregate.Commit(T1));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var refs = OneRef("History");

        var history = new object[]
        {
            new TransactionInitiatedEvent(id, "transfer", refs, T0)
        };

        var aggregate = (TransactionAggregate)Activator.CreateInstance(typeof(TransactionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.TransactionId);
        Assert.Equal(TransactionStatus.Initiated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
