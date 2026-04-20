using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public readonly record struct PlaybackWindow
{
    public Timestamp AvailableFrom { get; }
    public Timestamp AvailableUntil { get; }

    public PlaybackWindow(Timestamp availableFrom, Timestamp availableUntil)
    {
        Guard.Against(
            availableUntil.Value <= availableFrom.Value,
            "PlaybackWindow availableUntil must be after availableFrom.");
        AvailableFrom = availableFrom;
        AvailableUntil = availableUntil;
    }

    public bool Includes(Timestamp instant) =>
        instant.Value >= AvailableFrom.Value && instant.Value <= AvailableUntil.Value;
}
