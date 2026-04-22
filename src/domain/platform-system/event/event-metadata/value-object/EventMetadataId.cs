using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct EventMetadataId
{
    public Guid Value { get; }

    public EventMetadataId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EventMetadataId cannot be empty.");
        Value = value;
    }
}
