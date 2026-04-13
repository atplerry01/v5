namespace Whycespace.Shared.Contracts.Infrastructure.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// in-process read seam over the runtime's declared maintenance
/// state. Lives in shared/contracts so the runtime control plane
/// can consume the contract without referencing the host layer
/// (DG-R5-EXCEPT-01: runtime → host is forbidden).
///
/// Implementations MUST be thread-safe and non-blocking — the
/// control plane consults this provider on the per-command
/// dispatch hot path before the middleware pipeline runs.
/// </summary>
public interface IRuntimeMaintenanceModeProvider
{
    RuntimeMaintenanceMode Get();
}
