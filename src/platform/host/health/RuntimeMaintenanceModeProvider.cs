using Whyce.Shared.Contracts.Infrastructure.Health;

namespace Whyce.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// in-process concrete <see cref="IRuntimeMaintenanceModeProvider"/>.
/// Singleton-registered. Storage-only — a single
/// <see cref="RuntimeMaintenanceMode"/> reference flipped via
/// <see cref="Enter"/> / <see cref="Exit"/>. The state is read on
/// every command dispatch, so reads MUST be lock-free; writes are
/// rare (declared operator action) so the cost of a
/// <see cref="Volatile.Write"/> on entry/exit is negligible.
///
/// Default state: NOT in maintenance. The provider exposes
/// <see cref="Enter"/> / <see cref="Exit"/> as in-process methods so
/// any future operator endpoint or composition-time bootstrap can
/// flip the flag without HC-8 having to introduce a new HTTP
/// surface (out of scope per the HC-8 prompt — "no new admission
/// control / no infrastructure changes").
/// </summary>
public sealed class RuntimeMaintenanceModeProvider : IRuntimeMaintenanceModeProvider
{
    private RuntimeMaintenanceMode _current = RuntimeMaintenanceMode.None;

    public RuntimeMaintenanceMode Get() => Volatile.Read(ref _current);

    /// <summary>
    /// Declares the runtime as being in maintenance with one or
    /// more low-cardinality reason identifiers. The reason list is
    /// stored verbatim and surfaced on every subsequent
    /// <see cref="Get"/>. Calling <see cref="Enter"/> while already
    /// in maintenance replaces the reason list.
    /// </summary>
    public void Enter(params string[] reasons)
    {
        ArgumentNullException.ThrowIfNull(reasons);
        Volatile.Write(ref _current, new RuntimeMaintenanceMode(true, reasons));
    }

    /// <summary>
    /// Clears the maintenance posture. Calling <see cref="Exit"/>
    /// while not in maintenance is a no-op.
    /// </summary>
    public void Exit() => Volatile.Write(ref _current, RuntimeMaintenanceMode.None);
}
