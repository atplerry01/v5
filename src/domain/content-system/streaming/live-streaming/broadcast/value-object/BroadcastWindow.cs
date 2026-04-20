using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public readonly record struct BroadcastWindow
{
    public Timestamp ScheduledStart { get; }
    public Timestamp ScheduledEnd { get; }

    public BroadcastWindow(Timestamp scheduledStart, Timestamp scheduledEnd)
    {
        Guard.Against(
            scheduledEnd.Value <= scheduledStart.Value,
            "BroadcastWindow scheduledEnd must be after scheduledStart.");
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
    }

    public bool Includes(Timestamp instant) =>
        instant.Value >= ScheduledStart.Value && instant.Value <= ScheduledEnd.Value;
}
