using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

public readonly record struct SubtitleOutputRef
{
    public Guid Value { get; }

    public SubtitleOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SubtitleOutputRef cannot be empty.");
        Value = value;
    }
}
