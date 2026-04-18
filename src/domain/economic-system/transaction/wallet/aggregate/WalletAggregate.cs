using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public sealed class WalletAggregate : AggregateRoot
{
    public WalletId WalletId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid AccountId { get; private set; }
    public WalletStatus Status { get; private set; }
    public Timestamp CreatedAt { get; private set; }

    // T1.2 — Event-sourced idempotency guard for RequestTransaction.
    // Populated exclusively by applying TransactionRequestedEvent so replay
    // restores the dedupe set deterministically; no reliance on projections.
    private readonly HashSet<Guid> _processedRequestIds = new();
    public IReadOnlyCollection<Guid> ProcessedRequestIds => _processedRequestIds;

    private WalletAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static WalletAggregate Create(
        WalletId walletId,
        Guid ownerId,
        Guid accountId,
        Timestamp createdAt)
    {
        if (ownerId == Guid.Empty) throw WalletErrors.InvalidOwnerId();
        if (accountId == Guid.Empty) throw WalletErrors.InvalidAccountId();

        var aggregate = new WalletAggregate();
        aggregate.RaiseDomainEvent(new WalletCreatedEvent(walletId, ownerId, accountId, createdAt));
        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void RequestTransaction(
        Guid requestId,
        Guid destinationAccountId,
        Amount amount,
        Currency currency,
        Timestamp requestedAt)
    {
        if (requestId == Guid.Empty) throw WalletErrors.InvalidRequestId();
        if (Status != WalletStatus.Active) throw WalletErrors.WalletNotActive();
        if (AccountId == Guid.Empty) throw WalletErrors.NoAccountMapped();
        if (destinationAccountId == Guid.Empty) throw WalletErrors.InvalidDestination();
        if (amount.Value <= 0m) throw WalletErrors.InvalidAmount();

        // Deterministic idempotency: duplicate requestId is a no-op.
        // Retries within the projection-lag window (or across restarts)
        // yield a single ledger effect.
        if (_processedRequestIds.Contains(requestId))
            return;

        RaiseDomainEvent(new TransactionRequestedEvent(
            WalletId, requestId, AccountId, destinationAccountId, amount, currency, requestedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case WalletCreatedEvent e:
                WalletId = e.WalletId;
                OwnerId = e.OwnerId;
                AccountId = e.AccountId;
                Status = WalletStatus.Active;
                CreatedAt = e.CreatedAt;
                break;

            case TransactionRequestedEvent e:
                // State mutation is limited to recording the processed
                // request id so replay rebuilds the dedupe set.
                if (e.RequestId != Guid.Empty)
                    _processedRequestIds.Add(e.RequestId);
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Status == WalletStatus.Active && AccountId == Guid.Empty)
            throw WalletErrors.WalletMustHaveAccount();
    }
}
