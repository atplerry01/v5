using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public readonly record struct SubtitleId
{
    public Guid Value { get; }

    public SubtitleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SubtitleId cannot be empty.");
        Value = value;
    }
}
