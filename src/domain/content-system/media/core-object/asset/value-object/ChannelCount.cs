using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public readonly record struct ChannelCount
{
    public int Value { get; }

    public ChannelCount(int value)
    {
        Guard.Against(value < 1, "ChannelCount must be >= 1.");
        Guard.Against(value > 64, "ChannelCount cannot exceed 64.");
        Value = value;
    }

    public override string ToString() => Value.ToString();
}
