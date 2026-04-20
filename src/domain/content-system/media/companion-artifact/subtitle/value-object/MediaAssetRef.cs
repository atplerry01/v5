using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CompanionArtifact.Subtitle;

public readonly record struct MediaAssetRef
{
    public Guid Value { get; }

    public MediaAssetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaAssetRef cannot be empty.");
        Value = value;
    }
}
