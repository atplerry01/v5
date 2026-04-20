using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Transcript;

public readonly record struct TranscriptId
{
    public Guid Value { get; }

    public TranscriptId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TranscriptId cannot be empty.");
        Value = value;
    }
}
