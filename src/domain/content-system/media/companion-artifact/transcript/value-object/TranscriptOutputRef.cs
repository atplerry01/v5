using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Transcript;

public readonly record struct TranscriptOutputRef
{
    public Guid Value { get; }

    public TranscriptOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TranscriptOutputRef cannot be empty.");
        Value = value;
    }
}
