# TITLE
control-system — E2→E13 Full Vertical Delivery (Sections 7–13 of 18)

# CONTEXT
CLASSIFICATION: control-system
E1 domain layer is complete across all 7 contexts (35 domains at D2).
This prompt drives sections 7–13: contracts, T2E engine handlers, projections, API controllers, Kafka topics.
Final section (E: runtime wiring, F: tests) follows separately.

## Gap analysis (pre-execution)
- src/engines/T2E/control/          → does not exist
- src/shared/contracts/control/     → does not exist
- src/projections/control/          → does not exist
- src/platform/api/controllers/control/ → does not exist
- infrastructure/event-fabric/kafka/topics/control/ → does not exist

## ID bridge
IIdGenerator.Generate(seed) → Guid (32 hex chars).
Domain VOs require 64-char lowercase hex string.
Bridge in T2E handlers: `id.ToString("N").PadRight(64, '0')` — deterministic, valid.
Commands carry Guid aggregateId; handlers convert.

# OBJECTIVE
Implement sections 7–13 for all 35 control-system domains, batched by context.
Each batch delivers: contracts + T2E handlers + projections + API + Kafka topics.

# CONSTRAINTS
- No T1M for control-system (all operations are single-shot T2E; no multi-step compensation needed)
- Commands implement IHasAggregateId with Guid AggregateId
- T2E handlers: IEngine, no repository for factories (new aggregate), IAggregateRepository<T> for mutations
- Event schemas: flat records with Guid AggregateId (not typed VOs)
- Projections: IEnvelopeProjectionHandler + IProjectionHandler<TSchema> per event
- Controllers: inherit appropriate base class, [Authorize], [ApiController], [Route("api/control/...")]
- Kafka: 4 topics per domain (commands/events/retry/deadletter), prefix whyce.
- All code must pass all 4 guards (constitutional, runtime, domain, infrastructure)

# EXECUTION STEPS
Phase 2A: system-policy (6 domains)
Phase 2B: access-control (6 domains)
Phase 2C: configuration + audit (10 domains)
Phase 2D: observability + scheduling + system-reconciliation (13 domains)
Phase 2E: Runtime wiring (composition root, schema catalog, command dispatcher, EventDeserializer)
Phase 2F: Tests

# OUTPUT FORMAT
Per-domain: commands file, event schemas file, read model, policy IDs, T2E handlers, projection handler, reducer, controller, topics.json

# VALIDATION CRITERIA
- All guards pass (constitutional: no Guid.NewGuid/DateTime.UtcNow; domain: no DI imports; runtime: IEngine + IHasAggregateId; infra: topic naming)
- All 35 domains have complete vertical slice through API
- Runtime wiring connects all 35 domains to the dispatch pipeline
