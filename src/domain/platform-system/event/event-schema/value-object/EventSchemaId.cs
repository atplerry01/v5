using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public readonly record struct EventSchemaId
{
    public Guid Value { get; }

    public EventSchemaId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventSchemaId cannot be empty.");
        Value = value;
    }
}
