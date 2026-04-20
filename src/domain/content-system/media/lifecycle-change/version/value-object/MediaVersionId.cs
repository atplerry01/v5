using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.LifecycleChange.Version;

public readonly record struct MediaVersionId
{
    public Guid Value { get; }

    public MediaVersionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaVersionId cannot be empty.");
        Value = value;
    }
}
