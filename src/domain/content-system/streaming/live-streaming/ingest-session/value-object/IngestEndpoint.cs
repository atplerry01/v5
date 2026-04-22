using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

/// RTMP/SRT source endpoint URL for the broadcaster feed.
public readonly record struct IngestEndpoint
{
    public string Value { get; }

    public IngestEndpoint(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "IngestEndpoint cannot be empty.");
        Value = value.Trim();
    }
}
