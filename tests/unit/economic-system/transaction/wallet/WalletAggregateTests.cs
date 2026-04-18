using Whycespace.Domain.EconomicSystem.Transaction.Wallet;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Transaction.Wallet;

/// <summary>
/// T1.2 — Unit coverage for <see cref="WalletAggregate"/> idempotency guard.
///
/// RequestTransaction is event-sourced and must be safe under retry within
/// the projection-lag window. The aggregate tracks processed RequestIds in
/// its own state (rehydrated from TransactionRequestedEvent on replay) so
/// a duplicate request is a deterministic no-op: no additional event is
/// raised and downstream ledger effects stay exactly-once.
/// </summary>
public sealed class WalletAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp CreatedAt   = new(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));
    private static readonly Timestamp RequestedAt = new(new DateTimeOffset(2026, 4, 17, 12, 5, 0, TimeSpan.Zero));

    private static readonly Currency Usd = new("USD");

    private static WalletAggregate NewWallet(string seed)
    {
        var walletId   = new WalletId(IdGen.Generate($"WalletAggregateTests:{seed}:wallet"));
        var ownerId    = IdGen.Generate($"WalletAggregateTests:{seed}:owner");
        var accountId  = IdGen.Generate($"WalletAggregateTests:{seed}:account");
        var aggregate  = WalletAggregate.Create(walletId, ownerId, accountId, CreatedAt);
        aggregate.ClearDomainEvents();
        return aggregate;
    }

    [Fact]
    public void RequestTransaction_FirstInvocation_RaisesEventAndRecordsRequestId()
    {
        var wallet  = NewWallet("First");
        var destId  = IdGen.Generate("WalletAggregateTests:First:dest");
        var reqId   = IdGen.Generate("WalletAggregateTests:First:req");

        wallet.RequestTransaction(reqId, destId, new Amount(100m), Usd, RequestedAt);

        var evt = Assert.IsType<TransactionRequestedEvent>(Assert.Single(wallet.DomainEvents));
        Assert.Equal(reqId, evt.RequestId);
        Assert.Equal(destId, evt.DestinationAccountId);
        Assert.Contains(reqId, wallet.ProcessedRequestIds);
    }

    [Fact]
    public void RequestTransaction_DuplicateRequestId_IsDeterministicNoOp()
    {
        var wallet  = NewWallet("Dup");
        var destId  = IdGen.Generate("WalletAggregateTests:Dup:dest");
        var reqId   = IdGen.Generate("WalletAggregateTests:Dup:req");

        wallet.RequestTransaction(reqId, destId, new Amount(100m), Usd, RequestedAt);
        wallet.ClearDomainEvents();

        // Same RequestId replayed within projection-lag window.
        wallet.RequestTransaction(reqId, destId, new Amount(100m), Usd, RequestedAt);

        Assert.Empty(wallet.DomainEvents);
        Assert.Single(wallet.ProcessedRequestIds);
    }

    [Fact]
    public void RequestTransaction_DifferentRequestIds_EmitSeparateEvents()
    {
        var wallet  = NewWallet("Diff");
        var destId  = IdGen.Generate("WalletAggregateTests:Diff:dest");
        var reqA    = IdGen.Generate("WalletAggregateTests:Diff:reqA");
        var reqB    = IdGen.Generate("WalletAggregateTests:Diff:reqB");

        wallet.RequestTransaction(reqA, destId, new Amount(100m), Usd, RequestedAt);
        wallet.RequestTransaction(reqB, destId, new Amount(100m), Usd, RequestedAt);

        Assert.Equal(2, wallet.DomainEvents.Count);
        Assert.Equal(2, wallet.ProcessedRequestIds.Count);
        Assert.Contains(reqA, wallet.ProcessedRequestIds);
        Assert.Contains(reqB, wallet.ProcessedRequestIds);
    }

    [Fact]
    public void RequestTransaction_EmptyRequestId_Throws()
    {
        var wallet = NewWallet("Empty");
        var destId = IdGen.Generate("WalletAggregateTests:Empty:dest");

        Assert.ThrowsAny<DomainException>(() =>
            wallet.RequestTransaction(Guid.Empty, destId, new Amount(100m), Usd, RequestedAt));
    }

    [Fact]
    public void LoadFromHistory_RebuildsProcessedRequestIdSet()
    {
        var walletId   = new WalletId(IdGen.Generate("WalletAggregateTests:Replay:wallet"));
        var ownerId    = IdGen.Generate("WalletAggregateTests:Replay:owner");
        var accountId  = IdGen.Generate("WalletAggregateTests:Replay:account");
        var destId     = IdGen.Generate("WalletAggregateTests:Replay:dest");
        var reqId      = IdGen.Generate("WalletAggregateTests:Replay:req");

        var history = new object[]
        {
            new WalletCreatedEvent(walletId, ownerId, accountId, CreatedAt),
            new TransactionRequestedEvent(walletId, reqId, accountId, destId,
                new Amount(100m), Usd, RequestedAt)
        };

        var aggregate = (WalletAggregate)Activator.CreateInstance(typeof(WalletAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        // Replay of the same RequestId after rehydration is still a no-op.
        aggregate.RequestTransaction(reqId, destId, new Amount(100m), Usd, RequestedAt);

        Assert.Empty(aggregate.DomainEvents);
        Assert.Contains(reqId, aggregate.ProcessedRequestIds);
    }
}
