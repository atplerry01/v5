namespace Whycespace.Shared.Primitives.Time;

/// <summary>
/// Canonical time provider contract for WBSM v3 deterministic execution (GE-01).
/// Extends IClock with tick-level precision for seed composition.
///
/// All domain, engine, runtime, and systems code MUST use ITimeProvider or IClock
/// instead of DateTime.Now, DateTime.UtcNow, or DateTimeOffset.UtcNow.
///
/// Production: SystemTimeProvider (delegates to system clock)
/// Testing/Replay: DeterministicClock (manually advanced)
/// </summary>
public interface ITimeProvider : IClock
{
    long UtcNowTicks => UtcNow.Ticks;
}

/// <summary>
/// Production time provider — delegates to system clock.
/// Registered as singleton in DI container.
/// </summary>
public sealed class SystemTimeProvider : ITimeProvider
{
    public static readonly SystemTimeProvider Instance = new();

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
    public long UtcNowTicks => DateTime.UtcNow.Ticks;
}
