---
classification: economic-system
context: routing
domain-group: routing
domains: [execution, path]
executed-at: 2026-04-16
pipeline: /pipeline/** ($17 canonical)
source-context: /pipeline/execution_context_v3.md
---

# Project Prompt — Execute economic-system/routing batch

## TITLE
Phase 2 canonical execution of economic-system/routing/{execution, path} via `/pipeline/**`.

## CONTEXT
Input context v3 declared the batch: classification=economic-system, context=routing,
domain group=routing, domains={execution, path}. Pipeline $17 was followed end-to-end:
guard pre-load → execution_context_v3 → generic-prompt → generic-audit → (no fixes needed)
→ system audit sweep.

## OBJECTIVE
Bring the routing batch to full canonical end-to-end implementation per the WBSM v3
pipeline (API → Runtime → Engine → Domain → Event Store → Chain → Outbox → Kafka →
Projection → Response).

## CONSTRAINTS
- Guards: constitutional.guard.md, runtime.guard.md, domain.guard.md, infrastructure.guard.md
  loaded before execution per $1a.
- No new guard contradictions observed; no pause-and-ask triggered.
- Determinism preserved (no Guid.NewGuid, no DateTime.UtcNow in domain/engine; IClock +
  IIdGenerator everywhere).
- Topic naming canonical: `whyce.economic.routing.{execution,path}.{commands,events,retry,deadletter}`.

## EXECUTION STEPS (performed)
1. Inventoried existing routing footprint: domain (E1), contracts (E2/E3), T2E handlers (E4),
   schema binding (E6), projections (E7), API controllers (E8), composition (all present).
2. Identified three missing infrastructure artifacts: Kafka topic JSON descriptors,
   create-topics.sh entries (K-TOPIC-COVERAGE-01 S0), rego policy files.
3. Created the three missing artifacts; no C# source changes required.
4. Ran pipeline audit (generic-audit.md) — 12 stages PASS, no fix pass needed.
5. Ran system audit sweep (constitutional/runtime/domain/infrastructure) — clean.

## OUTPUT FORMAT
Batch summary returned inline to the user. Pipeline execution record persisted here.

## VALIDATION CRITERIA
- All 8 routing topics present in `create-topics.sh`.
- `topics.json` descriptors exist for both sub-domains.
- Rego policies exist and are default-deny with `valid_resource` scope guards.
- No determinism / domain-purity / config-safety violations in new files.
- Existing routing wiring verified against all four canonical guards.
