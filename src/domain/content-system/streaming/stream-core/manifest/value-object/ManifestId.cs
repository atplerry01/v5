using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Manifest;

public readonly record struct ManifestId
{
    public Guid Value { get; }

    public ManifestId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ManifestId cannot be empty.");
        Value = value;
    }
}
