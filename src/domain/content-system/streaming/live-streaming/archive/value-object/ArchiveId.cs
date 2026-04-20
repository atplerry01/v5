using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

public readonly record struct ArchiveId
{
    public Guid Value { get; }

    public ArchiveId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ArchiveId cannot be empty.");
        Value = value;
    }
}
