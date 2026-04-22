using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public readonly record struct SerializationFormatId
{
    public Guid Value { get; }

    public SerializationFormatId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SerializationFormatId cannot be empty.");
        Value = value;
    }
}
