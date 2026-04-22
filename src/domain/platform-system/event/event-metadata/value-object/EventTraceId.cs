using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public readonly record struct EventTraceId
{
    public string Value { get; }

    public EventTraceId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EventTraceId cannot be empty.");
        Value = value;
    }
}
