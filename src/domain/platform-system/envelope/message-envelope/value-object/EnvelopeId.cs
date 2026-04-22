using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public readonly record struct EnvelopeId
{
    public Guid Value { get; }

    public EnvelopeId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EnvelopeId cannot be empty.");
        Value = value;
    }
}
