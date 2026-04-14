# PHASE 2 — ECONOMIC-SYSTEM — CAPITAL FULL WIRING (OPTION C)

## TITLE
Economic-System Capital — E1 → EX → Platform API → Kafka → Projection full wiring

## CONTEXT
Captured 2026-04-14 per WBSM v3 $2. Original prompt requested a flat `CapitalAggregate` with
Create/Credit/Debit/Reserve/Release/Freeze/Unfreeze. Pre-execution guard/structure survey
($1a) revealed that `src/domain/economic-system/capital/` is already S4-implemented with
eight bounded contexts: account, allocation, asset, binding, pool, reserve, vault.
User locked in Option C: complete the E1→EX→API→Kafka→Projection wiring for the existing
contexts without altering domain structure.

## CLASSIFICATION
economic / capital / {account, allocation, asset, binding, pool, reserve, vault}

## OBJECTIVE
Bring the capital domain to full operational readiness by implementing command contracts,
T2E handlers, Kafka topics, projection reducers, and API endpoints for every existing
capital context. Do not add new aggregates. Do not collapse or modify the existing
eight-context decomposition.

## CONSTRAINTS
- $5 anti-drift: no aggregate rename, move, or re-decomposition.
- $7 layer purity: domain unchanged; engines stateless; runtime owns persist/publish/anchor.
- $9 determinism: no Guid.NewGuid, no DateTime.UtcNow — IIdGenerator + IClock only.
- $10 events: handlers emit only; naming `whyce.economic.capital.{context}.*`.
- CLASS-SFX-R1/R2: no `-system` suffix outside `src/domain/`.
- No `src/application/` layer — commands live in `src/shared/contracts/`; handlers in `src/engines/T2E/economic/capital/{context}/`.
- Capital is NEVER source of truth — ledger is. Capital state is derived/enforced.
- Balance invariants: balance never negative; reserved ≤ available + reserved; cannot debit
  beyond available; frozen accounts cannot mutate.
- E2E validation: code-complete only. Runtime E2E marked pending.

## EXECUTION STEPS
1. Pattern recon: sibling handler (revenue/distribution), projection reducer (revenue),
   API controller (EconomicController), kafka topics script, command contract layout.
2. Enumerate each aggregate's public operations → one command per operation.
3. Add command contracts under `src/shared/contracts/economic/capital/{context}/`.
4. Add T2E handlers under `src/engines/T2E/economic/capital/{context}/`.
5. Add projection reducers + read models under `src/projections/economic/capital/{context}/`.
6. Extend `infrastructure/event-fabric/kafka/create-topics.sh` with four topics per context.
7. Extend `src/platform/api/controllers/economic/EconomicController.cs` with capital endpoints
   (or add a sibling controller if size warrants).
8. Run audit sweep per $1b; capture any new rules per $1c.

## OUTPUT FORMAT
Source files committed to the paths above; final summary lists files created and pending runtime validation.

## VALIDATION CRITERIA
- All handlers compile against `IEngine` / `IEngineContext` interfaces.
- All commands pass through `IWorkflowDispatcher` from API to runtime.
- No handler calls persistence; all emit events via `context.EmitEvents`.
- All events prefixed `whyce.economic.capital.{context}.`.
- No new `-system` suffixes outside `src/domain/`.
- Audit sweep PASS or documented deviations in `/claude/new-rules/`.