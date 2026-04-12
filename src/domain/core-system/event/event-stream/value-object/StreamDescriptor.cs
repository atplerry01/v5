namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public readonly record struct StreamDescriptor
{
    public string StreamName { get; }
    public string AggregateType { get; }

    public StreamDescriptor(string streamName, string aggregateType)
    {
        if (string.IsNullOrWhiteSpace(streamName))
            throw new ArgumentException("StreamName must not be null or empty.", nameof(streamName));
        if (string.IsNullOrWhiteSpace(aggregateType))
            throw new ArgumentException("AggregateType must not be null or empty.", nameof(aggregateType));

        StreamName = streamName;
        AggregateType = aggregateType;
    }
}
