using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct EventSpanId
{
    public string Value { get; }

    public EventSpanId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventSpanId cannot be empty.");
        Value = value;
    }
}
