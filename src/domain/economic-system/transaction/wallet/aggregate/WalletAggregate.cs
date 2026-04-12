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
        Guid destinationAccountId,
        Amount amount,
        Currency currency,
        Timestamp requestedAt)
    {
        if (Status != WalletStatus.Active) throw WalletErrors.WalletNotActive();
        if (AccountId == Guid.Empty) throw WalletErrors.NoAccountMapped();
        if (destinationAccountId == Guid.Empty) throw WalletErrors.InvalidDestination();
        if (amount.Value <= 0m) throw WalletErrors.InvalidAmount();

        RaiseDomainEvent(new TransactionRequestedEvent(
            WalletId, AccountId, destinationAccountId, amount, currency, requestedAt));
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

            case TransactionRequestedEvent:
                // No state mutation — the request is a signal to the transaction context
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
