using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ConstitutionalSystem.Chain.AnchorRecord;

public sealed class AnchorRecordAggregate : AggregateRoot
{
    public AnchorRecordId Id { get; private set; }
    public AnchorDescriptor Descriptor { get; private set; } = null!;
    public AnchorRecordStatus Status { get; private set; }
    public DateTimeOffset AnchoredAt { get; private set; }
    public DateTimeOffset? SealedAt { get; private set; }

    private AnchorRecordAggregate() { }

    public static AnchorRecordAggregate Record(
        AnchorRecordId id,
        AnchorDescriptor descriptor,
        DateTimeOffset anchoredAt)
    {
        if (string.IsNullOrWhiteSpace(descriptor.BlockHash))
            throw AnchorRecordErrors.InvalidBlockHash();
        if (descriptor.Sequence < 0)
            throw AnchorRecordErrors.InvalidSequence();

        var aggregate = new AnchorRecordAggregate();
        aggregate.RaiseDomainEvent(new AnchorRecordCreatedEvent(id, descriptor, anchoredAt));
        return aggregate;
    }

    public void Seal(DateTimeOffset sealedAt)
    {
        if (!AnchorNotSealedSpecification.IsSatisfiedBy(Status))
            throw AnchorRecordErrors.AlreadySealed();

        RaiseDomainEvent(new AnchorRecordSealedEvent(Id, sealedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AnchorRecordCreatedEvent e:
                Id = e.AnchorRecordId.Value == Guid.Empty
                    ? new AnchorRecordId(AggregateIdentity)
                    : e.AnchorRecordId;
                Descriptor = e.Descriptor;
                AnchoredAt = e.AnchoredAt;
                Status = AnchorRecordStatus.Created;
                break;

            case AnchorRecordSealedEvent e:
                Status = AnchorRecordStatus.Sealed;
                SealedAt = e.SealedAt;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value == Guid.Empty, "AnchorRecord must have a valid Id.");
        Guard.Against(Descriptor is null, "AnchorRecord must have a descriptor.");
    }
}
