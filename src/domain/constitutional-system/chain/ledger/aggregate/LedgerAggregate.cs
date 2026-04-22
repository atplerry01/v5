using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed class LedgerAggregate : AggregateRoot
{
    public LedgerId Id { get; private set; }
    public LedgerDescriptor Descriptor { get; private set; } = null!;
    public LedgerStatus Status { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? SealedAt { get; private set; }

    private LedgerAggregate() { }

    public static LedgerAggregate Open(
        LedgerId id,
        LedgerDescriptor descriptor,
        DateTimeOffset openedAt)
    {
        if (string.IsNullOrWhiteSpace(descriptor.LedgerName))
            throw LedgerErrors.EmptyLedgerName();

        var aggregate = new LedgerAggregate();
        aggregate.RaiseDomainEvent(new LedgerOpenedEvent(id, descriptor, openedAt));
        return aggregate;
    }

    public void Seal(DateTimeOffset sealedAt)
    {
        if (!LedgerNotSealedSpecification.IsSatisfiedBy(Status))
            throw LedgerErrors.AlreadySealed();

        RaiseDomainEvent(new LedgerSealedEvent(Id, sealedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case LedgerOpenedEvent e:
                Id = e.LedgerId.Value == Guid.Empty
                    ? new LedgerId(AggregateIdentity)
                    : e.LedgerId;
                Descriptor = e.Descriptor;
                OpenedAt = e.OpenedAt;
                Status = LedgerStatus.Open;
                break;

            case LedgerSealedEvent e:
                Status = LedgerStatus.Sealed;
                SealedAt = e.SealedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value == Guid.Empty, "Ledger must have a valid Id.");
        Guard.Against(Descriptor is null, "Ledger must have a descriptor.");
    }
}
