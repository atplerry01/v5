using Whyce.Shared.Kernel.Domain;

namespace Whycespace.Tests.Shared;

/// <summary>
/// Frozen clock for deterministic tests. Returns the same UtcNow on every call.
/// Used by replay tests to guarantee envelope timestamp equality across runs.
/// </summary>
public sealed class TestClock : IClock
{
    public static readonly DateTimeOffset Frozen = new(2026, 4, 7, 12, 0, 0, TimeSpan.Zero);
    public DateTimeOffset UtcNow => Frozen;
}
