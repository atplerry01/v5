# DEPENDENCY GRAPH AUDIT

## CLASSIFICATION
system / governance / dependency-control

## PRIORITY
S0

## PURPOSE
Post-execution verification that the canonical layer dependency rules
hold across the repository. The full rule set is R1–R7 plus the DG-*
additions in `dependency-graph.guard.md`: `DG-R5-EXCEPT-01`
(composition-root permission), `DG-R5-HOST-DOMAIN-FORBIDDEN`
(host→domain prohibition, strengthened §5.1.2 Step C-G to cover
fully-qualified and aliased forms), and `DG-R7-01` (projections→runtime,
remediated 2026-04-07). `scripts/dependency-check.sh` is the
authoritative mechanical enforcement.

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
Grep `using` directives. The `src/platform` predicate is split between
`platform/api` (default rule R5) and `platform/host` (composition root
under DG-R5-EXCEPT-01) to match `scripts/dependency-check.sh`:
```
src/domain         → must NOT contain Runtime|Engines|Systems|Platform|Infrastructure|Projections
src/engines        → must NOT contain Runtime|Systems|Platform|Infrastructure|Projections
src/runtime        → must NOT contain Systems|Platform|Projections
src/systems        → must NOT contain Engines|Platform|Infrastructure|Projections
src/platform/api   → must NOT contain Runtime|Engines|Domain|Projections
src/platform/host  → must NOT contain Whycespace.Domain (DG-R5-HOST-DOMAIN-FORBIDDEN, three forms);
                     runtime/engines/systems/projections usings ARE permitted under DG-R5-EXCEPT-01
src/projections    → must NOT contain Engines|Runtime|Systems|Platform
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

## CURRENT BASELINE (2026-04-08, Phase 1.5 §5.1.1 post-remediation)

Baseline updated after Phase 1.5 §5.1.1 Steps B + C. The 2026-04-07 baseline
listed below in the **HISTORICAL** block is preserved for traceability.

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
Status: PASS
  - src/projections/Whycespace.Projections.csproj → references {Whycespace.Shared} only.
  - DG-R7-01 (projections → runtime) remediated 2026-04-07 via shared
    `IEnvelopeProjectionHandler` / `IEventEnvelope` contracts.
  - 2026-04-08 verification (Phase 1.5 §5.1.1 Step B): no `using Whyce.{Runtime,
    Engines,Systems,Platform}` and no `using Whycespace.Domain.*` found in
    `src/projections/**`. Step B is verification-only; no code changes required.

Layer: platform/api
Status: PASS

Layer: platform/host
Status: PASS (with documented exception)
  - src/platform/host/Whycespace.Host.csproj → references {Api, Shared, Runtime,
    Engines, Systems, Projections}. All four cross-layer edges (runtime, engines,
    systems, projections) are JUSTIFIED composition-root edges under
    DG-R5-EXCEPT-01 (DI registration + host adapter implementation).
  - host → domain edge REMOVED 2026-04-08 (Phase 1.5 §5.1.1 Step C). The sole
    typed usage in src/platform/host/adapters/PostgresOutboxAdapter.cs was
    replaced with reflection-based unwrap of the `.Value` property; the
    `<ProjectReference>` to Whycespace.Domain.csproj was removed from
    Whycespace.Host.csproj.
  - DG-R5-HOST-DOMAIN-FORBIDDEN now blocks any reintroduction.
  - 2026-04-08 (Phase 1.5 §5.1.2 Step C-G): predicate strengthened.
    DG-R5-HOST-DOMAIN-FORBIDDEN clauses 3 and 4 now also forbid
    fully-qualified `Whycespace.Domain.X.Y` expressions and namespace
    aliases of the form `using <Alias> = Whycespace.Domain.<…>;`.
    `scripts/dependency-check.sh` enforces the strengthened predicate
    over `src/platform/host/**`, excluding pure-comment lines so the
    canonical intent-comment in
    `src/platform/host/composition/runtime/RuntimeComposition.cs:80`
    remains valid documentation. Eleven prior bypass sites in
    `src/platform/host/composition/**` were remediated under §5.1.2
    BPV-D01 (relocated to `src/runtime/event-fabric/domain-schemas/**`).
```

```
=== DEPENDENCY GRAPH AUDIT (BASELINE 2026-04-08) ===
Date:        2026-04-08
Status:      PASS
Verified by: Phase 1.5 §5.1.1 final verification (2026-04-08)
             - dotnet build src/platform/host/Whycespace.Host.csproj:
               0 warnings, 0 errors, full 8-project closure built
             - scripts/dependency-check.sh: 0 violations, exit 0
             - host → domain absent at csproj and using-level
             - projections → runtime absent at csproj and using-level
Workstream:  Phase 1.5 §5.1.1 Dependency Graph Remediation — CLOSED
```

---

## HISTORICAL BASELINE (2026-04-07) — for traceability only

```
Layer: projections
Status: FAIL
Violations:
  - src/projections/Whycespace.Projections.csproj → references Whycespace.Runtime (R7)

Layer: platform/host
Status: FAIL
Violations:
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Runtime  (R5)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Engines  (R5)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Projections (R5)
  - src/platform/host/Whycespace.Host.csproj → references Whycespace.Domain   (R5)
```

Resolution map:
- projections → runtime: REMEDIATED 2026-04-07 (DG-R7-01 in guard).
- host → runtime / engines / systems / projections: RECLASSIFIED 2026-04-07
  as documented exception DG-R5-EXCEPT-01 (composition-root DI scope).
- host → domain: REMEDIATED 2026-04-08 (Phase 1.5 §5.1.1 Step C). Removed
  from DG-R5-EXCEPT-01 permitted list and forbidden by DG-R5-HOST-DOMAIN-FORBIDDEN.

---

## FAILURE ACTION
Any FAIL → halt downstream prompt execution per CLAUDE.md $1b. Audit report
must be addressed by an explicit remediation prompt before lock.

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-DG-R7-01**: ~~scripts/dependency-check.sh MUST flag Whycespace.Projections.csproj -> Whycespace.Runtime.csproj~~ → **CLOSED 2026-04-07**. Projections csproj no longer references runtime; verified again 2026-04-08 in Phase 1.5 §5.1.1 Step B.
- **CHECK-DG-R5-01**: ~~scripts/dependency-check.sh MUST flag Whycespace.Host.csproj -> Runtime/Engines/Projections/Domain as 4 distinct R5 violations~~ → **PARTIALLY CLOSED**. host → runtime/engines/projections reclassified as DI-only under DG-R5-EXCEPT-01 (2026-04-07). host → domain REMOVED 2026-04-08. Replaced by CHECK-DG-HOST-DOMAIN-01 below.
- **CHECK-DG-BASELINE-01**: ~~baseline scan findings MUST remain RED~~ → **CLOSED 2026-04-08**. Baseline updated; resolutions recorded under HISTORICAL BASELINE section.

## NEW CHECKS INTEGRATED — 2026-04-08 (Phase 1.5 §5.1.1 Step E)

- **CHECK-DG-HOST-DOMAIN-01** (S0): `scripts/dependency-check.sh` MUST fail
  if `src/platform/host/Whycespace.Host.csproj` contains a `<ProjectReference>`
  to `Whycespace.Domain.csproj`, OR if any file under `src/platform/host/**`
  contains `using Whycespace.Domain` at any depth. Enforces
  DG-R5-HOST-DOMAIN-FORBIDDEN.
