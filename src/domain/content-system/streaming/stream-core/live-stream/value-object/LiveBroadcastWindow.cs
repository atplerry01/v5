using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.LiveStream;

public readonly record struct LiveBroadcastWindow
{
    public Timestamp ScheduledStart { get; }
    public Timestamp ScheduledEnd { get; }

    public LiveBroadcastWindow(Timestamp scheduledStart, Timestamp scheduledEnd)
    {
        Guard.Against(
            scheduledEnd.Value <= scheduledStart.Value,
            "LiveBroadcastWindow scheduledEnd must be after scheduledStart.");
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
    }

    public bool Includes(Timestamp instant) =>
        instant.Value >= ScheduledStart.Value && instant.Value <= ScheduledEnd.Value;
}
