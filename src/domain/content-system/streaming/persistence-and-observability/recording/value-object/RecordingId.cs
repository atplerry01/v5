using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Recording;

public readonly record struct RecordingId
{
    public Guid Value { get; }

    public RecordingId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "RecordingId cannot be empty.");
        Value = value;
    }
}
