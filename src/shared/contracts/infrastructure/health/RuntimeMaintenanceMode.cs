namespace Whyce.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// immutable view of the runtime's declared maintenance posture.
/// Carries the deterministic boolean and a low-cardinality reason
/// list. Maintenance mode is a HARD BLOCK — when active, the
/// runtime control plane refuses every command BEFORE the
/// middleware pipeline runs. There is no degraded-style "tag and
/// proceed" branch here; maintenance is the explicit operator
/// switch that takes the runtime out of service for a declared
/// reason.
/// </summary>
public sealed record RuntimeMaintenanceMode(
    bool IsMaintenance,
    IReadOnlyList<string> Reasons)
{
    /// <summary>
    /// Allocation-free "not in maintenance" sentinel — safe for
    /// the dispatch hot path.
    /// </summary>
    public static RuntimeMaintenanceMode None { get; } =
        new(false, Array.Empty<string>());
}
