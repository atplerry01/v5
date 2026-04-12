using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Binding;

public sealed class BindingAggregate : AggregateRoot
{
    public BindingId BindingId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid OwnerId { get; private set; }
    public OwnershipType OwnershipType { get; private set; }
    public BindingStatus Status { get; private set; }
    public Timestamp BoundAt { get; private set; }

    private BindingAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static BindingAggregate Bind(
        BindingId bindingId,
        Guid accountId,
        Guid ownerId,
        OwnershipType ownershipType,
        Timestamp boundAt)
    {
        Guard.Against(accountId == Guid.Empty, "Account ID cannot be empty.");
        Guard.Against(ownerId == Guid.Empty, "Owner ID cannot be empty.");

        var aggregate = new BindingAggregate();

        aggregate.RaiseDomainEvent(new CapitalBoundEvent(
            bindingId,
            accountId,
            ownerId,
            ownershipType,
            boundAt));

        return aggregate;
    }

    // ── Behavior ─────────────────────────────────────────────────

    public void TransferOwnership(Guid newOwnerId, OwnershipType newType, Timestamp transferredAt)
    {
        if (Status == BindingStatus.Released)
            throw BindingErrors.CannotTransferReleasedBinding();

        if (Status == BindingStatus.Transferred)
            throw BindingErrors.BindingAlreadyTransferred();

        if (Status != BindingStatus.Active)
            throw BindingErrors.BindingNotActive();

        Guard.Against(newOwnerId == Guid.Empty, "New owner ID cannot be empty.");
        Guard.Against(newOwnerId == OwnerId, "New owner must differ from current owner.");

        RaiseDomainEvent(new OwnershipTransferredEvent(
            BindingId,
            OwnerId,
            newOwnerId,
            newType,
            transferredAt));
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
            AccountId,
            releasedAt));
    }

    // ── Event Sourcing ───────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CapitalBoundEvent e:
                BindingId = e.BindingId;
                AccountId = e.AccountId;
                OwnerId = e.OwnerId;
                OwnershipType = e.OwnershipType;
                Status = BindingStatus.Active;
                BoundAt = e.BoundAt;
                break;

            case OwnershipTransferredEvent e:
                OwnerId = e.NewOwnerId;
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
            Guard.Against(AccountId == Guid.Empty, "AccountId must not be empty for an active binding.");
            Guard.Against(OwnerId == Guid.Empty, "OwnerId must not be empty for an active binding.");
        }
    }
}
