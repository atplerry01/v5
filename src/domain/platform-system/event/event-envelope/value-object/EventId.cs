using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventEnvelope;

public readonly record struct EventId
{
    public Guid Value { get; }

    public EventId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventId cannot be empty.");
        Value = value;
    }
}
