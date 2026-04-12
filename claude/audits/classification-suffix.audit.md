# Classification Suffix Audit

## C1 — Domain Suffix Check

**Scan:** `src/domain/**` (top-level classification folders)
**Assert:** All classification folders end with `-system`.
**Status:** PASS

## C2 — Non-Domain Suffix Violation

**Scan:** `src/{engines,runtime,systems,platform,projections,shared}/**`
**Assert:** No path contains `-system`.
**Status:** RESOLVED

Projection layer classification suffix violation remediated (2026-04-10):
- `src/projections/governance-system` → `src/projections/governance`
- `src/projections/identity-system` → `src/projections/identity`
- `src/projections/orchestration-system` → `src/projections/orchestration`

All non-domain layers are now classification-pure.

## C3 — Event Naming Check

**Scan:** Domain events, runtime event fabric, Kafka topics.
**Assert:** No event contains `-system`.
**Status:** PASS

## C4 — Routing Consistency

**Scan:** DomainRoute, SystemIntentDispatcher, Controllers.
**Assert:** All routing uses `classification/context/domain` (no `-system`).
**Status:** PASS
