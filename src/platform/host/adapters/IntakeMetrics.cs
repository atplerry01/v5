using System.Diagnostics.Metrics;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-1 (INTAKE-CONFIG-01): dedicated meter for the
/// runtime HTTP intake admission limiter. Registered counters are
/// exported via any active <see cref="MeterListener"/> (the host wires
/// Prometheus via prometheus-net.AspNetCore).
///
/// Naming follows the canonical <c>intake.*</c> namespace so the meter
/// is unambiguous in any scrape and the three counters are explicit
/// about what they measure:
///
///   - <c>intake.admitted</c>      — request acquired a permit and proceeded.
///   - <c>intake.rejected</c>      — request refused at the limiter (queue full
///                                   or per-partition ceiling exceeded).
///   - <c>intake.queue.full</c>    — request specifically refused because the
///                                   per-partition queue itself was full
///                                   (subset of intake.rejected).
///
/// Tags: <c>partition</c> ∈ { "tenant", "ip" } so a saturated tenant is
/// distinguishable from a saturated IP fan-in. <c>route</c> is added
/// when a controller route can be identified before the limiter runs
/// (currently absent — the limiter runs before routing has bound a
/// controller, by design).
/// </summary>
public static class IntakeMetrics
{
    public static readonly Meter Meter = new("Whycespace.Intake", "1.0");

    public static readonly Counter<long> Admitted =
        Meter.CreateCounter<long>("intake.admitted");

    public static readonly Counter<long> Rejected =
        Meter.CreateCounter<long>("intake.rejected");

    public static readonly Counter<long> QueueFull =
        Meter.CreateCounter<long>("intake.queue.full");
}
