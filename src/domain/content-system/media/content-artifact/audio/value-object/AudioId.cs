using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public readonly record struct AudioId
{
    public Guid Value { get; }

    public AudioId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AudioId cannot be empty.");
        Value = value;
    }
}
