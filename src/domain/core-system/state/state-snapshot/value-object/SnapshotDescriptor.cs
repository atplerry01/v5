using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public readonly record struct SnapshotDescriptor
{
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public long SequenceNumber { get; }

    public SnapshotDescriptor(Guid aggregateId, string aggregateType, long sequenceNumber)
    {
        Guard.Against(aggregateId == Guid.Empty, "SnapshotDescriptor requires a non-empty AggregateId.");
        Guard.Against(string.IsNullOrWhiteSpace(aggregateType), "SnapshotDescriptor requires a non-blank AggregateType.");
        Guard.Against(sequenceNumber <= 0, "SnapshotDescriptor requires SequenceNumber > 0.");

        AggregateId = aggregateId;
        AggregateType = aggregateType;
        SequenceNumber = sequenceNumber;
    }
}
