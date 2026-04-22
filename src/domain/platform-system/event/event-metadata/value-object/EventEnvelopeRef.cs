using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct EventEnvelopeRef
{
    public Guid Value { get; }

    public EventEnvelopeRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventEnvelopeRef cannot be empty.");
        Value = value;
    }
}
