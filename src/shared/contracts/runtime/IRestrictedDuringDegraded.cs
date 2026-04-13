namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// phase1.5-S5.2.4 / HC-8 (MAINTENANCE-MODE-ENFORCEMENT-01):
/// command-side marker indicating that the command MUST be
/// rejected when the runtime is in a Degraded posture. Lives in
/// <c>Whycespace.Shared.Contracts.Runtime</c> alongside
/// <see cref="CommandContext"/> so any command in any
/// <c>shared/contracts/application/*</c> namespace can opt in
/// without taking a host or runtime reference.
///
/// Semantics:
///   - Commands that DO NOT implement this marker continue to
///     dispatch normally under Degraded; <c>CommandContext.IsExecutionRestricted</c>
///     is set so observability/audit can correlate.
///   - Commands that DO implement this marker are HARD BLOCKED
///     by the runtime control plane when degraded, with the
///     deterministic reason <c>"restricted_during_degraded_mode"</c>.
///
/// HC-8 introduces no commands that implement this marker — the
/// marker is an opt-in signal that future workstreams can apply
/// to specific non-critical commands. The enforcement gate works
/// today; the population of restricted commands is deferred.
/// </summary>
public interface IRestrictedDuringDegraded
{
}
