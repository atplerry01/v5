using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public readonly record struct ArchiveOutputRef
{
    public Guid Value { get; }

    public ArchiveOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ArchiveOutputRef cannot be empty.");
        Value = value;
    }
}
