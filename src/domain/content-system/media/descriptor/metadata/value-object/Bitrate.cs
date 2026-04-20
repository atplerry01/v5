using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct Bitrate
{
    public long BitsPerSecond { get; }

    public Bitrate(long bitsPerSecond)
    {
        Guard.Against(bitsPerSecond < 0, "Bitrate cannot be negative.");
        BitsPerSecond = bitsPerSecond;
    }

    public override string ToString() => $"{BitsPerSecond} bps";
}
