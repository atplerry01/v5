# Classification Suffix Guard

## CLASS-SFX-R1 — Domain Suffix Required

All classification folders directly under `src/domain/` MUST end with `-system`.

**Scan:** `src/domain/` (top-level directories only, excluding `bin`, `obj`, `shared-kernel`)
**Assert:** Every classification folder name ends with `-system`.
**Severity:** S0 (build blocking)

## CLASS-SFX-R2 — Non-Domain Suffix Prohibited

Only `src/domain/**` may contain `-system` in folder names.
All other `src/**` directories MUST NOT contain `-system`.

**Scan:** `src/{engines,runtime,systems,platform,projections,shared}/**`
**Assert:** No directory path segment ends with `-system`.
**Severity:** S0 (build blocking)

## CLASS-SFX-R3 — Enforcement

Violations of CLASS-SFX-R1 or CLASS-SFX-R2 are S0 severity.
Any prompt that would introduce a `-system` suffix outside `src/domain/` or remove one inside `src/domain/` MUST be halted before execution.
