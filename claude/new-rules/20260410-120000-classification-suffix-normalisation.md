# Classification Suffix Normalisation

**CLASSIFICATION:** structural
**SOURCE:** Classification Suffix Audit — C2 failure
**DESCRIPTION:** The projection layer incorrectly used the `-system` suffix in three folder names (`governance-system`, `identity-system`, `orchestration-system`). The `-system` suffix is reserved exclusively for domain classification folders under `src/domain/`.
**PROPOSED_RULE:** CLASS-SFX-R2 — Only `src/domain/**` may contain `-system`. All other `src/**` directories MUST NOT contain `-system`. Promoted to `claude/guards/classification-suffix.guard.md`.
**SEVERITY:** S0

## Root Cause

Projection layer folders were created mirroring domain classification naming (`-system` suffix) instead of using the classification-pure naming required for non-domain layers.

## Fix

- Renamed `src/projections/governance-system` → `src/projections/governance`
- Renamed `src/projections/identity-system` → `src/projections/identity`
- Renamed `src/projections/orchestration-system` → `src/projections/orchestration`
- Updated `Whyce.Projections.OrchestrationSystem` namespace → `Whyce.Projections.Orchestration`
- Updated bootstrap using statement in `WorkflowExecutionBootstrap.cs`

## Guard

CLASS-SFX-R2 added to `claude/guards/classification-suffix.guard.md` to prevent recurrence.

## Impact

Prevents cross-layer classification drift. Ensures only domain layer carries the `-system` suffix.
