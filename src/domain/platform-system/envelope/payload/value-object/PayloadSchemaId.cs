using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public readonly record struct PayloadSchemaId
{
    public Guid Value { get; }

    public PayloadSchemaId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PayloadSchemaId cannot be empty.");
        Value = value;
    }
}
