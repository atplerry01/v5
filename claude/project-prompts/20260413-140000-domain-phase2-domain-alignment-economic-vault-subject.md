---
classification: domain
context: economic-system
domain: vault, subject, capital
phase: 2
type: structural-alignment
captured: 2026-04-13
---

# PHASE 2 — DOMAIN MODEL ALIGNMENT (CANONICAL, NO DRIFT)

## CONTEXT

Whycespace WBSM v3.5 repository. Domain layer only. Structural alignment
pass to make the economic-system domain reflect the locked doctrine of:

- 4-slice vault with Slice 1 as the sole liquidity gateway
- Slice 1 → Slice 2 investment transition
- SPV profit → Slice 1 return path
- Vault metrics invariant: Total = Free + Locked + Invested
- Structural ↔ economic identity bridge (economic subject)

## OBJECTIVE

Align and extend `src/domain/economic-system/` to the doctrine. Classify
existing domains (KEEP / RESHAPE / DEFER / DEAD), introduce missing
domains (`vault/*`, `subject/subject`), define aggregate + value-object
placement, and state doctrine-driven domain responsibilities.

## CONSTRAINTS

- Canonical topology: `{classification-system}/{context}/{domain}` (3 levels).
- No deletions of existing domains unless proven dead.
- No flattening, no new top-level folders, no naming changes.
- No runtime / no engine / no API / no projection / no handler.
- DOMAIN LAYER ONLY. Structure and model definition, not implementation.
- Respect domain guard: all seven artifact folders per domain
  (`aggregate/entity/error/event/service/specification/value-object`).
- Money remains a shared-kernel primitive (guard rule 21).

## EXECUTION STEPS

1. Discover current `src/domain/economic-system/` topology.
2. Classify every existing domain KEEP / RESHAPE / DEFER / DEAD.
3. Introduce mandatory new domains: `vault/account`, `vault/slice`,
   `vault/metrics`, `subject/subject`.
4. Place aggregates in their canonical domains.
5. Place value objects in their canonical domains.
6. State domain responsibilities and doctrine invariants.
7. Emit the structural report only — no code.

## OUTPUT FORMAT

1. Updated domain structure tree (target end-state)
2. Classification report (KEEP / RESHAPE / ADD / DEFER / DEAD)
3. Aggregate placement
4. Value-object placement
5. Responsibility + invariant summary

## VALIDATION CRITERIA

- Every new path matches `economic-system/{context}/{domain}`.
- No violation of Domain Structure Guard (DS-R1..R8).
- No violation of Domain Guard rules 1, 2, 11, 16, 17, 18, 19, 21.
- Money placement does not duplicate shared-kernel primitive.
- No aggregate/value object proposed outside a canonical domain folder.
- No implementation code emitted.
