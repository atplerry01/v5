using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public readonly record struct StreamDescriptor
{
    public string StreamName { get; }
    public string AggregateType { get; }

    public StreamDescriptor(string streamName, string aggregateType)
    {
        Guard.Against(string.IsNullOrWhiteSpace(streamName), "StreamName must not be null or empty.");
        Guard.Against(string.IsNullOrWhiteSpace(aggregateType), "AggregateType must not be null or empty.");

        StreamName = streamName;
        AggregateType = aggregateType;
    }
}
