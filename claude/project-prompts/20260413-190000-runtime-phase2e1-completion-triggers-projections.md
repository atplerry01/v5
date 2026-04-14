---
classification: runtime
context: economic
domain: revenue/{revenue,distribution,payout}, vault/account
phase: 2E.1
type: runtime-completion
captured: 2026-04-13
---

# PHASE 2E.1 — RUNTIME COMPLETION (KAFKA → TRIGGER / PROJECTIONS)

## OBJECTIVE

Close the Phase 2E scaffolding to a fully event-driven runtime:

- Kafka → WorkflowTriggerHandler via a new `WorkflowTriggerWorker`.
- Kafka → projection materialization for revenue, distribution, payout, vault/account
  via four `GenericKafkaProjectionConsumerWorker` instances.
- Typed event schemas + read models + reducers + projection stores per domain.

## CONSTRAINTS

- No domain or workflow-step changes.
- Reuse existing Kafka infrastructure (Confluent.Kafka + projection worker pattern).
- Build must remain 0 warnings / 0 errors.

## DEFERRED

- Live end-to-end test (requires Kafka + Postgres + OPA running).
- Failure-injection tests (duplicate payout, partial failure, Kafka restart replay).
