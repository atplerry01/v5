using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public readonly record struct HeaderSchemaId
{
    public Guid Value { get; }

    public HeaderSchemaId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "HeaderSchemaId cannot be empty.");
        Value = value;
    }
}
