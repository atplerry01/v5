using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.Escalation;

/// <summary>
/// Time-bucketed, replay-deterministic accumulation window. Start + Duration
/// are set when the escalation is opened; IsExpired is a pure predicate of
/// a caller-supplied timestamp (no clock reads inside the aggregate).
/// </summary>
public readonly record struct EscalationWindow
{
    public Timestamp Start { get; }
    public TimeSpan Duration { get; }

    public EscalationWindow(Timestamp start, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        Start = start;
        Duration = duration;
    }

    public static readonly TimeSpan DefaultDuration = TimeSpan.FromHours(24);

    public static EscalationWindow Open(Timestamp start) => new(start, DefaultDuration);

    public bool IsExpired(Timestamp now) => now.Value - Start.Value >= Duration;
}
