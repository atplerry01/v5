using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventStream;

public readonly record struct EventStreamId
{
    public Guid Value { get; }

    public EventStreamId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventStreamId cannot be empty.");
        Value = value;
    }
}
