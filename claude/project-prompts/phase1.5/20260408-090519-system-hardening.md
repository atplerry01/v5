# WBSM v3.5 — Full System Hardening Batch

CLASSIFICATION: system / governance / hardening
MODE: STRICT — gated campaign (H0 discovery → H1–H11)
RECEIVED: 2026-04-08 09:05:19
EXECUTION MODE: gated, audit-checkpointed (user-approved override of single-pass directive)

## Objective
Achieve full WBSM v3.5 alignment before Phase 2 (Economic Core) via additive
hardening on top of phase1 gates S0–S8. No new architecture, no abstractions,
no helper creation. Halt if required helpers absent.

## Gates
- H0  Discovery (no mutation) — MANDATORY
- H1  Determinism purge (Guid.NewGuid / DateTime.UtcNow → IIdGenerator / IClock)
- H2  Projection consolidation (handlers → src/projections)
- H3  Dependency graph enforcement
- H4  Engine purity (no DB/Kafka/Redis in engines)
- H5  Workflow boundary (WSS defines, T1M executes, runtime orchestrates)
- H6  Event fabric hard lock (all writes via IEventFabric)
- H7  Kafka optimization
- H8  Event store optimization
- H9  Projection optimization
- H10 Structural cleanup
- H11 Final validation

## Required Helpers (must pre-exist)
- IClock
- IIdGenerator (canonical) / DeterministicIdHelper / IDeterministicIdEngine
- IEventFabric.ProcessAsync + ProcessAuditAsync
- IEngineContext.EmitEvents

## Per-Gate Contract
1. Audit output  2. Diff report  3. Tests run  4. Commit-safe (one commit per gate)
5. New rules captured under /claude/new-rules/ when applicable

## Validation (H11)
- 0 Guid.NewGuid in domain
- 0 DateTime.UtcNow in domain
- 0 projection handlers in runtime
- 0 DB calls in engines
- 0 direct Kafka publish outside EventFabric/outbox
- Dependency graph compliant
- E2E flow functional
