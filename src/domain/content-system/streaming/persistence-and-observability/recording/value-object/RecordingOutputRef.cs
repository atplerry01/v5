using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public readonly record struct RecordingOutputRef
{
    public Guid Value { get; }

    public RecordingOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RecordingOutputRef cannot be empty.");
        Value = value;
    }
}
