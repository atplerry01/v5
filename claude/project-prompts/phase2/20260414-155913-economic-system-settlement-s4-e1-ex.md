# TITLE
PHASE 2 — ECONOMIC-SYSTEM — SETTLEMENT DOMAIN (S4 + E1→EX IMPLEMENTATION)

# CONTEXT
Establishes the external financial execution boundary under the canonical binding
`economic-system:transaction:settlement`. Follows WBSM v3 layer purity:
domain owns truth, runtime persists/anchors, engines emit events only,
platform exposes POST endpoints dispatched via workflow runtime.

Complements (does not replace) the existing `economic-system/ledger/settlement`
internal-truth layer. This transaction-context settlement models external
execution records (bank rails, payment providers) whose completion/failure
is authoritative for off-platform value movement.

# CLASSIFICATION
- classification: economic-system
- context:        transaction
- domain:         settlement

# LOCKED EXECUTION DECISIONS (prompt-normative, applied per CLAUDE.md $15)
1. Domain path: `src/domain/economic-system/transaction/settlement/`
   (three-level nesting enforced by $7; original prompt's 2-level
   `economic-system/settlement/` would violate structural purity and
   misalign with every sibling domain under transaction/).
2. E1 "Application" commands: `src/shared/contracts/economic/transaction/settlement/`
   (no `src/application/` layer exists; creating one would violate $5 anti-drift
   and $6 file-system whitelist. Placement follows the established
   `ExpenseCommands.cs` / `DistributionCommands.cs` precedents).
3. EX handlers: `src/engines/T2E/economic/transaction/settlement/`
   (engine root is `economic`, not `economic-system` — established pattern).
4. Projection: `src/projections/economic/transaction/settlement/`
   (same `economic` root naming).
5. Kafka topics: `whyce.economic.transaction.settlement.{commands|events|retry|deadletter}`.
6. Event schemas: `src/shared/contracts/events/economic/transaction/settlement/SettlementEventSchemas.cs`.
7. Coexistence: the pre-existing `src/domain/economic-system/ledger/settlement/`
   (internal-truth variant) is NOT touched per $5. Transaction-level settlement
   is the external-execution boundary; ledger-level settlement is internal
   posting reconciliation.
8. E2E runtime verification (API / Kafka round-trip / projection / chain anchor)
   is marked PENDING — code-complete per the ledger-phase precedent; end-to-end
   exercise requires the phase-2 runtime wiring.

# OBJECTIVE
Deliver a full S4 settlement domain with aggregate, entity, value objects,
events, specifications, errors, service, and README; wire E1
(InitiateSettlementCommand / CompleteSettlementCommand / FailSettlementCommand),
EX (T2E handlers), a read-model projection, Kafka topics, and an API
controller exposing initiate / complete / fail endpoints.

# CONSTRAINTS
## DOMAIN
- No persistence; no Guid.NewGuid; no DateTime.UtcNow.
- Deterministic IDs only (SHA256 via IIdGenerator at the entry point).
- Settlement is reference-based, not execution-based — domain never calls rails.
- Settlement is irreversible at domain level (Completed/Failed are terminal).
- Emit events only; no cross-aggregate mutation.

## ECONOMIC
- Settlement must reference a valid source (ledger entry / transaction / capital).
- Settlement cannot mutate once Completed or Failed.
- Settlement lifecycle strictly enforced by `SettlementLifecycleSpecification`.
- Settlement status is authoritative for external execution tracking.

## ENGINE (T2E)
- Stateless; returns events only; no DB / Kafka / Redis / HTTP / SDK access.

## PROJECTION
- Idempotent upsert via `last_event_id`.
- Depends ONLY on shared contracts; never imports domain.

## LIFECYCLE
```
Initiated -> Processing -> Completed (terminal)
Initiated -> Processing -> Failed    (terminal)
```

# EXECUTION STEPS
1. Domain S4 tree: value-object, entity, event, spec, error, service, aggregate, README.
2. Shared contracts: SettlementCommands, SettlementReadModel, SettlementEventSchemas.
3. T2E handlers: InitiateSettlementHandler / CompleteSettlementHandler / FailSettlementHandler.
4. Projection: SettlementProjectionHandler + SettlementProjectionReducer.
5. Kafka topics in `infrastructure/event-fabric/kafka/create-topics.sh`.
6. Platform API: SettlementController with POST initiate / complete / fail.
7. Build-verify; capture E2E readiness notes.
8. Guard + audit sweep per $1a / $1b.

# OUTPUT FORMAT
Full folder tree, new + modified files, build result, audit summary, E2E
readiness notes.

# VALIDATION CRITERIA
- Three-level nesting respected.
- No Guid.NewGuid / DateTime.UtcNow in domain.
- No persistence, no external SDK calls in engine.
- Projection free of domain imports.
- Topic names canonical (`whyce.economic.transaction.settlement.*`).
- Event records follow `{Domain}{Action}Event` (Settlement{Initiated|Processing|Completed|Failed}Event).
- Lifecycle strictly irreversible: no transition out of Completed or Failed.
- Source reference mandatory on initiation.
- README present and complete.
- All guard files PASS.
