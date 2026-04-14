---
classification: domain
context: economic-system
domain: subject, vault, capital
phase: 2B
type: implementation
captured: 2026-04-13
---

# PHASE 2B — ECONOMIC DOMAIN IMPLEMENTATION (CANONICAL, CORRECTED)

## CONTEXT

Whycespace WBSM v3.5 — implement the economic-system domain layer only.
Corrects Phase 2 by collapsing vault into a single-aggregate model:
`VaultAccountAggregate` owns `VaultSliceEntity`[4] and a `VaultMetrics`
value object. No slice aggregate. No metrics aggregate. No metrics events.

## OBJECTIVE

Implement these domains:

- economic-system/subject/subject
- economic-system/vault/account
- economic-system/vault/slice
- economic-system/vault/metrics
- economic-system/capital/account (already D2 — no structural change)
- economic-system/capital/allocation (already D2 — no structural change)
- economic-system/capital/reserve (already D2 — no structural change)

## CONSTRAINTS

- Single aggregate in vault context: `VaultAccountAggregate`.
- Slice is an `Entity`, placed under `vault/slice/entity/`.
- Metrics is a value object at `vault/metrics/value-object/VaultMetrics`.
- Invariant `Total = Free + Locked + Invested` enforced in VaultMetrics ctor.
- Business-level events only. Allowed: `VaultAccountCreatedEvent`,
  `VaultFundedEvent`, `CapitalAllocatedToSliceEvent`. Forbidden: slice-level
  credit/debit events, metrics events.
- Funding accepted ONLY by Slice1. Investment is Slice1 → Slice2.
- No engines, no projections, no controllers, no runtime.

## EXECUTION STEPS

1. Scaffold `subject/subject/` (aggregate + VOs + event + error).
2. Scaffold `vault/account/` (aggregate + VOs + 3 events + error).
3. Scaffold `vault/slice/` (entity + SliceType VO; no aggregate).
4. Scaffold `vault/metrics/` (VaultMetrics VO; no aggregate).
5. Populate `.gitkeep` in unused mandatory folders.
6. Capital domains remain unchanged.

## OUTPUT FORMAT

1. Full folder structure of new domains.
2. Aggregates, entities, value objects, events, specifications.
3. List of files written.

## VALIDATION CRITERIA

- DS-R1..R8 clean for new paths.
- DG rules 1, 2, 6, 14, 15, 21, 23 clean.
- `Money`/`Amount`/`Currency` sourced from shared-kernel.
- VaultMetrics invariant enforced on construction.
- Only 3 vault events exist.
- No metrics or low-level slice events.
