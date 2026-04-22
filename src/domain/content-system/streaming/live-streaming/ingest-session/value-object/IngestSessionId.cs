using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public readonly record struct IngestSessionId
{
    public Guid Value { get; }

    public IngestSessionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "IngestSessionId cannot be empty.");
        Value = value;
    }
}
