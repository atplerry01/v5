using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct TrustScore
{
    public int Value { get; }

    public TrustScore(int value)
    {
        Guard.Against(value < 0 || value > 100, "TrustScore must be in range [0, 100].");
        Value = value;
    }
}
