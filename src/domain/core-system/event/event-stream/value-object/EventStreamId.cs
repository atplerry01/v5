using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public readonly record struct EventStreamId
{
    public Guid Value { get; }

    public EventStreamId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventStreamId value must not be empty.");
        Value = value;
    }
}
