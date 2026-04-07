---
classification: system / governance / dependency-control
source: dependency-graph.guard.md baseline scan (2026-04-07)
severity: S0
---

# CLASSIFICATION
Architectural — layer dependency violations.

# SOURCE
Initial baseline run of `dependency-graph.guard.md` + `dependency-check.sh`
against current `dev_wip` branch.

# DESCRIPTION
Two pre-existing layer violations found by the new dependency guard:

1. **Projections → Runtime (R7 violation)**
   `src/projections/Whycespace.Projections.csproj` declares
   `<ProjectReference Include="..\runtime\Whycespace.Runtime.csproj" />`.
   Projections may only reference `domain` (events) + `shared`.

2. **Platform/Host fan-in (R5 violation, ×4)**
   `src/platform/host/Whycespace.Host.csproj` references `Runtime`,
   `Engines`, `Projections`, and `Domain` directly. Under R5, platform
   may only reference `systems` (and `shared`). Composition root may
   warrant a documented exception, but none is currently granted.

# PROPOSED_RULE
Either:
- (a) Remediate: invert dependencies so Projections consume domain events
  via a contracts surface in `shared` or `domain`, and route Host
  composition exclusively through `systems` facades; OR
- (b) Grant explicit, documented exceptions in `dependency-graph.guard.md`
  for the composition root (Host) and event-shape access (Projections),
  scoped narrowly (e.g., interface-only, no implementation types).

# SEVERITY
S0 — Architectural. Blocks LOCK condition for the dependency graph.

# ACTION_REQUIRED
Authorized remediation prompt. Per CLAUDE.md $5 (Anti-Drift), no
architectural change applied automatically.
