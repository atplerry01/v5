namespace Whycespace.Domain.CoreSystem.State.StateSnapshot;

public readonly record struct SnapshotDescriptor
{
    public Guid AggregateId { get; }
    public string AggregateType { get; }
    public long SequenceNumber { get; }

    public SnapshotDescriptor(Guid aggregateId, string aggregateType, long sequenceNumber)
    {
        if (aggregateId == Guid.Empty)
            throw new InvalidOperationException("SnapshotDescriptor requires a non-empty AggregateId.");

        if (string.IsNullOrWhiteSpace(aggregateType))
            throw new InvalidOperationException("SnapshotDescriptor requires a non-blank AggregateType.");

        if (sequenceNumber <= 0)
            throw new InvalidOperationException("SnapshotDescriptor requires SequenceNumber > 0.");

        AggregateId = aggregateId;
        AggregateType = aggregateType;
        SequenceNumber = sequenceNumber;
    }
}
