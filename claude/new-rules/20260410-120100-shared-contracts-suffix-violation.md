# Shared Contracts Classification Suffix Violation

**CLASSIFICATION:** structural
**SOURCE:** Classification Suffix Audit — discovered during projection-layer remediation
**DESCRIPTION:** The shared contracts layer contains `-system` suffix in two folder paths, violating CLASS-SFX-R2. These were not in the original C2 audit scan scope (`src/shared/` was excluded) but are now captured by the guard.
**PROPOSED_RULE:** Already covered by CLASS-SFX-R2 in `claude/guards/classification-suffix.guard.md`.
**SEVERITY:** S0

## Violations

- `src/shared/contracts/events/orchestration-system/`
- `src/shared/contracts/projections/orchestration-system/`

## Note

These folders contain contract namespaces (`Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow`, `Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow`) referenced by multiple layers. Remediation requires a coordinated rename across shared, projections, runtime, engines, and platform — a separate prompt execution.
