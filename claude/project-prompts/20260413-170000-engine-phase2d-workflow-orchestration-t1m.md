---
classification: engine
context: economic
domain: revenue (revenue, distribution, payout) + vault (account)
phase: 2D
type: workflow-orchestration
captured: 2026-04-13
---

# PHASE 2D — T1M WORKFLOW ORCHESTRATION (ECONOMIC)

## CONTEXT

Scaffolds T1M workflows for revenue normalization, distribution creation,
and payout execution. Adds the required T2E handlers + shared-contract
commands. Canonical path: `classification/context/domain` with raw
classification (no `-system`) in non-domain layers.

## OBJECTIVE

- Workflow "economic.revenue.process" → validate → dispatch ApplyRevenueCommand
- Workflow "economic.distribution.create" → validate → dispatch CreateDistributionCommand
- Workflow "economic.payout.execute" → load shares → per-share Debit/Credit + conservation

## CONSTRAINTS

- Steps never touch domain aggregates directly.
- All execution via `ISystemIntentDispatcher.DispatchAsync(command, route, ct)`.
- Typed state via `context.SetState<T>()` / `GetState<T>()`.
- No structural-system imports.
- Commands in `shared/contracts/economic/...`; handlers in `engines/T2E/economic/...`.
