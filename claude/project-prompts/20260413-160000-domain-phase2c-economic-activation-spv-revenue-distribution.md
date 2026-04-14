---
classification: domain
context: economic-system
domain: capital/allocation, vault/account, revenue/revenue, revenue/distribution, revenue/payout
phase: 2C
type: implementation
captured: 2026-04-13
---

# PHASE 2C — ECONOMIC ACTIVATION (SPV + REVENUE + DISTRIBUTION)

## CONTEXT

Activate the SPV → Revenue → Distribution → Payout flow within the
economic-system domain. No engines, no runtime, no API — domain only.

## OBJECTIVE

- Extend CapitalAllocation with SPV ownership (TargetType, TargetId, OwnershipPercentage).
- Record revenue against an SPV via RevenueAggregate.
- Normalize SPV revenue into vault Slice1 via VaultAccountAggregate.ApplyRevenue.
- Compute participant shares from ownership percentages in DistributionAggregate.
- Execute payout by debiting SPV Slice1 and crediting each participant's Slice1.

## CONSTRAINTS

- Only extend specified domains.
- Strong types where required (SliceType enum in events, Amount/Currency from shared-kernel).
- Events: intent-based, no metrics, no state snapshots.
- No structural-system imports — use EconomicSubject bridge.
- Invariants: ownership sum = 100, payout conservation, revenue → Slice1 only.
