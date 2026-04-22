namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

public enum IngestSessionStatus
{
    Authenticated,
    Streaming,
    Stalled,
    Ended,
    Failed
}
