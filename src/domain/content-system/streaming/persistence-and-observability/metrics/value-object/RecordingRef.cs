using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public readonly record struct RecordingRef
{
    public Guid Value { get; }

    public RecordingRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RecordingRef cannot be empty.");
        Value = value;
    }
}
