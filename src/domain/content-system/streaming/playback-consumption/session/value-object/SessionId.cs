using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

public readonly record struct SessionId
{
    public Guid Value { get; }

    public SessionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SessionId cannot be empty.");
        Value = value;
    }
}
