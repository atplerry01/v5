using Whycespace.Domain.EconomicSystem.Capital.Account;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class BindingAggregate : AggregateRoot
{
    public BindingId BindingId { get; private set; }
    public AccountId AccountId { get; private set; }
    public OwnerId OwnerId { get; private set; }
    public OwnershipType OwnershipType { get; private set; }
    public BindingStatus Status { get; private set; }
    public Timestamp BoundAt { get; private set; }

    private BindingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static BindingAggregate Bind(
        BindingId bindingId,
        AccountId accountId,
        OwnerId ownerId,
        OwnershipType ownershipType,
        Timestamp boundAt)
    {
        var aggregate = new BindingAggregate();

        aggregate.RaiseDomainEvent(new CapitalBoundEvent(
            bindingId,
            accountId.Value,
            ownerId.Value,
            ownershipType,
            boundAt));

        return aggregate;
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed refs.
    public static BindingAggregate Bind(
        BindingId bindingId,
        Guid accountId,
        Guid ownerId,
        OwnershipType ownershipType,
        Timestamp boundAt)
    {
        Guard.Against(accountId == Guid.Empty, "Account ID cannot be empty.");
        Guard.Against(ownerId == Guid.Empty, "Owner ID cannot be empty.");
        return Bind(bindingId, new AccountId(accountId), new OwnerId(ownerId), ownershipType, boundAt);
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void TransferOwnership(OwnerId newOwnerId, OwnershipType newType, Timestamp transferredAt)
    {
        if (Status == BindingStatus.Released)
            throw BindingErrors.CannotTransferReleasedBinding();

        if (Status == BindingStatus.Transferred)
            throw BindingErrors.BindingAlreadyTransferred();

        if (Status != BindingStatus.Active)
            throw BindingErrors.BindingNotActive();

        Guard.Against(newOwnerId.Value == OwnerId.Value, "New owner must differ from current owner.");

        RaiseDomainEvent(new OwnershipTransferredEvent(
            BindingId,
            OwnerId.Value,
            newOwnerId.Value,
            newType,
            transferredAt));
    }

    // D-ID-REF-01 dual-path: legacy Guid overload normalizes to typed ref.
    public void TransferOwnership(Guid newOwnerId, OwnershipType newType, Timestamp transferredAt)
    {
        Guard.Against(newOwnerId == Guid.Empty, "New owner ID cannot be empty.");
        TransferOwnership(new OwnerId(newOwnerId), newType, transferredAt);
    }

    public void Release(Timestamp releasedAt)
    {
        if (Status == BindingStatus.Transferred)
            throw BindingErrors.CannotReleaseTransferredBinding();

        if (Status == BindingStatus.Released)
            throw BindingErrors.BindingAlreadyReleased();

        if (Status != BindingStatus.Active)
            throw BindingErrors.BindingNotActive();

        RaiseDomainEvent(new BindingReleasedEvent(
            BindingId,
            AccountId.Value,
            releasedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CapitalBoundEvent e:
                BindingId = e.BindingId;
                AccountId = new AccountId(e.AccountId);
                OwnerId = new OwnerId(e.OwnerId);
                OwnershipType = e.OwnershipType;
                Status = BindingStatus.Active;
                BoundAt = e.BoundAt;
                break;

            case OwnershipTransferredEvent e:
                OwnerId = new OwnerId(e.NewOwnerId);
                OwnershipType = e.NewOwnershipType;
                Status = BindingStatus.Transferred;
                break;

            case BindingReleasedEvent:
                Status = BindingStatus.Released;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (Status == BindingStatus.Active)
        {
            Guard.Against(AccountId.Value == Guid.Empty, "AccountId must not be empty for an active binding.");
            Guard.Against(OwnerId.Value == Guid.Empty, "OwnerId must not be empty for an active binding.");
        }
    }
}
