using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public readonly record struct EventEnvelopeId
{
    public Guid Value { get; }

    public EventEnvelopeId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventEnvelopeId cannot be empty.");
        Value = value;
    }
}
