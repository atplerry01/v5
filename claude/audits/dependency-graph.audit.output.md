# DEPENDENCY GRAPH AUDIT — OUTPUT

**Date:** 2026-04-08
**Sweep ID:** 20260408-132840
**Scope:** all .csproj + using directives in src/, tests/
**Verdict:** PASS

---

## FINDINGS

**Zero illegal references. Zero cycles.** All R1–R7 rules from dependency-graph.guard.md hold.

### DG-INFO-01 — dependency-check.sh false positives (script bug, not code)
- **File:** scripts/dependency-check.sh
- **Severity:** S3 (tooling)
- **Issue 1:** Script flags `/src/shared/obj/*.json` (build artifacts) as shared-kernel I/O leaks. Needs to exclude `/obj/` and `/bin/` before scanning.
- **Issue 2:** Script flags 7 domain files under `src/domain/business-system/integration/adapter/**` as "infrastructure adapters outside allowed paths" purely on filename `*Adapter*.cs`. These are domain aggregates modeling the *concept* of an Adapter (bounded context = Integration), not infrastructure adapters. Refine the check to look for `IEventStore`/`IProjectionWriter`/`IRepository` implementation markers instead of filenames.
- **Fix:** Script-only refinement. Code is clean.

---

## RESOLVED PRIOR FINDINGS
- **DG-R7-01** (Projections.csproj → Runtime.csproj): RESOLVED. Projections now references only Shared.
- **DG-R5-01** (Platform/Host fan-in): DOCUMENTED EXCEPTION DG-R5-EXCEPT-01 — composition root + adapter implementations only.

## RESOLVED CSPROJ MATRIX

```
shared          → (none)
domain          → (none)
engines         → domain, shared
runtime         → engines, domain, shared
systems         → shared
projections     → shared
platform/api    → systems, shared
platform/host   → api, shared, runtime*, engines*, systems, projections*, domain*
                  *DG-R5-EXCEPT-01: composition root + adapters only
```

No cycles. Acyclic DAG verified by DFS.

## CHECKLIST
- [x] R1 Domain zero-deps
- [x] R2 Engines stateless
- [x] R3 Runtime persistence boundary
- [x] R4 Projections one-way
- [x] R5 Platform composition exception scoped
- [x] R6 Systems leaf
- [x] R7 No cycles
- [x] WbsmArchitectureTests still passing
