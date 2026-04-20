namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public enum LiveStreamStatus
{
    Created,
    Scheduled,
    Live,
    Paused,
    Ended,
    Cancelled
}
