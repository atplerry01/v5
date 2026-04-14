---
classification: runtime
context: economic
domain: revenue, distribution, payout, vault/account
phase: 2E
type: runtime-activation
captured: 2026-04-13
---

# PHASE 2E — RUNTIME ACTIVATION (ECONOMIC)

## CONTEXT

End-to-end activation of the economic flow: host wiring, Kafka topics,
API endpoints, projection scaffolds, and event-to-workflow trigger.
Domain and engine logic remain frozen.

## OBJECTIVE

- Wire Phase 2D composition modules into the host via `EconomicCompositionRoot`.
- Register in `BootstrapModuleCatalog.All`.
- Add Kafka topics for economic.revenue.{revenue,distribution,payout} + economic.vault.account.
- Create three POST endpoints at `/api/economic/*`.
- Scaffold projection handlers for the three new economic events.
- Scaffold a `WorkflowTriggerHandler` that routes `RevenueRecordedEvent` and
  `PayoutExecutedEvent` to their workflows via `IWorkflowDispatcher`.

## CONSTRAINTS

- No domain or engine changes.
- Determinism, policy, audit middleware preserved (no changes to dispatcher chain).
- Idempotency inherited automatically from `IdempotencyMiddleware` — deterministic
  `CommandId` derived from `ToString()` signature (no command field additions needed).
- Build must remain 0/0 across all projects.

## DEFERRED (this phase scaffolds; next phase wires)

- Runtime Kafka consumer worker attachment of `WorkflowTriggerHandler` to
  `whyce.economic.*.events` topics.
- Typed event-schema + read-model + reducer + `PostgresProjectionStore`
  wiring for the three projection handlers.
- `EconomicCompositionRoot.RegisterSchema` / `RegisterProjections` population.
- End-to-end runtime tests (require Kafka + Postgres + OPA running).
- Failure-injection tests (Kafka-down, Postgres-down, OPA-down, duplicate command).
