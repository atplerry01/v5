# DEPENDENCY GRAPH AUDIT

## CLASSIFICATION
system / governance / dependency-control

## PRIORITY
S0

## PURPOSE
Post-execution verification that the canonical layer dependency rules
(R1–R7 in `dependency-graph.guard.md`) hold across the repository.

---

## INPUTS
- All `src/**/*.csproj`
- All `src/**/*.cs` (`using` directives)
- `scripts/dependency-check.sh` exit code (when wired)

---

## CHECKS

### C1 — Project Reference Graph
Parse every `<ProjectReference>` in `src/**/*.csproj`. For each layer, assert
references conform to the matrix in `dependency-graph.guard.md`.

### C2 — Namespace Usage Violations
Grep `using` directives:
```
src/domain      → must NOT contain Runtime|Engines|Systems|Platform|Infrastructure|Projections
src/engines     → must NOT contain Runtime|Systems|Platform|Infrastructure|Projections
src/runtime     → must NOT contain Systems|Platform|Projections
src/systems     → must NOT contain Engines|Platform|Infrastructure|Projections
src/platform    → must NOT contain Runtime|Engines|Domain|Projections
src/projections → must NOT contain Engines|Runtime|Systems|Platform
```

### C3 — Circular Dependencies
Build directed graph from project references. Assert no cycles via DFS.

### C4 — Adapter Leakage
Find files matching `*Adapter*.cs` outside `src/platform/host/adapters/**`
and `src/infrastructure/**`.

### C5 — Shared Misuse
Inside `src/shared/**`, fail on any reference to runtime/engine/infrastructure
namespaces or any I/O type (Npgsql, Kafka, Redis, HTTP, file system).

---

## OUTPUT FORMAT

```
Layer: <name>
Status: PASS | FAIL
Violations:
  - <file>:<line> → <description>
```

Followed by a summary block:

```
=== DEPENDENCY GRAPH AUDIT ===
Date:        <YYYY-MM-DD HH:MM:SS>
Layers:      shared, domain, engines, runtime, systems, projections, platform
Total Refs:  <n>
Cycles:      <n>
Violations:  <n>
Status:      PASS | FAIL
```

---

## CURRENT BASELINE (2026-04-07)

Baseline scan recorded at guard introduction. Existing deviations are listed
here for transparency; remediation requires its own authorized prompt
(CLAUDE.md $5 anti-drift).

```
Layer: shared
Status: PASS

Layer: domain
Status: PASS

Layer: engines
Status: PASS

Layer: runtime
Status: PASS

Layer: systems
Status: PASS

Layer: projections
Status: FAIL
Violations:
  - src/projections/Whycespace.Projections.csproj → references Whycespace.Runtime (R7: projections → domain + shared only)

Layer: platform/api
Status: PASS

Layer: platform/host
Status: FAIL
Violations:
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Runtime  (R5: platform → systems only)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Engines  (R5)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Projections (R5)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Domain   (R5)
```

```
=== DEPENDENCY GRAPH AUDIT (BASELINE) ===
Date:        2026-04-07
Status:      FAIL (5 baseline violations — see above)
```

These violations are captured as new rules / drift findings; see
`/claude/new-rules/`.

---

## FAILURE ACTION
Any FAIL → halt downstream prompt execution per CLAUDE.md $1b. Audit report
must be addressed by an explicit remediation prompt before lock.

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-DG-R7-01**: scripts/dependency-check.sh MUST flag Whycespace.Projections.csproj -> Whycespace.Runtime.csproj as a violation until remediated or explicit exception is logged in dependency-graph.guard.md.
- **CHECK-DG-R5-01**: scripts/dependency-check.sh MUST flag Whycespace.Host.csproj -> Runtime/Engines/Projections/Domain as 4 distinct R5 violations until remediated or scoped exception is granted. LOCK condition is suspended while either rule is red.
- **CHECK-DG-BASELINE-01**: (2026-04-07 addendum) The baseline scan findings above MUST remain RED in the audit report until the new-rule
  `claude/new-rules/_archives/20260407-160000-dependency-graph.md` is resolved (remediation PR or documented exception). S0.
