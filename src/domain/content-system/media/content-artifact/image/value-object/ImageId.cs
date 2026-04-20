using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public readonly record struct ImageId
{
    public Guid Value { get; }

    public ImageId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ImageId cannot be empty.");
        Value = value;
    }
}
