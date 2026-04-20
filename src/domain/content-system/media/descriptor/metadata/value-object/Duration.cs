using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct Duration
{
    public long Milliseconds { get; }

    public Duration(long milliseconds)
    {
        Guard.Against(milliseconds < 0, "Duration cannot be negative.");
        Milliseconds = milliseconds;
    }

    public override string ToString() => $"{Milliseconds} ms";
}
