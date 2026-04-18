using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Phase 8 B7 — mutable test clock that supports mid-test advancement.
///
/// <para>
/// Distinct from <see cref="TestClock"/> (a frozen-at-construction clock)
/// because expiry-scheduler tests need to simulate time passing — e.g.
/// "issue a sanction with ExpiresAt = now + 1h, advance clock 2h, assert
/// the scheduler picks it up". The underlying field is a plain
/// <see cref="DateTimeOffset"/> so reads and writes are single-writer
/// per-test-thread.
/// </para>
/// </summary>
public sealed class AdvanceableClock : IClock
{
    public static readonly DateTimeOffset Seed =
        new(2026, 4, 17, 12, 0, 0, TimeSpan.Zero);

    private DateTimeOffset _now;

    public AdvanceableClock() => _now = Seed;

    public AdvanceableClock(DateTimeOffset seed) => _now = seed;

    public DateTimeOffset UtcNow => _now;

    public void Advance(TimeSpan delta) => _now = _now.Add(delta);

    public void AdvanceTo(DateTimeOffset moment) => _now = moment;
}
