# platform.audit.md

Post-execution audit for the platform/host composition root. Pairs with
`claude/guards/infrastructure.guard.md` §Platform Boundaries (R-PLAT-*).

## SCOPE
- `src/platform/host/**`
- `src/platform/api/**`

## CHECKS

### CHECK-PLAT-NO-DOMAIN-01 — Program.cs domain leakage (S1)
grep `src/platform/host/Program.cs` for any concrete domain type
(`Whycespace.Domain.*`, `*Engine`, `*ProjectionHandler`, `*Schema` keyed by
domain). Per-domain wiring MUST be encapsulated in a `*Bootstrap.cs` module
under `src/platform/host/composition/` (or `src/systems/`) and invoked from
`Program.cs` by a single non-typed call (e.g. `TodoBootstrap.Register(builder)`).
Any domain reference in Program.cs = S1.

### CHECK-PLAT-RESOLVER-01 — Static type dictionaries (S1)
grep `src/platform/host/adapters/**` for static dictionaries keyed by domain
types (`Dictionary<string, Type>` literals containing `Whycespace.Domain.*`).
Any hit = S1. Mappings MUST come from a runtime-side `EventSchemaRegistry`.

### CHECK-PLAT-KAFKA-GENERIC-01 — Per-domain Kafka workers (S1)
Any class under `src/platform/host/adapters/**` whose name encodes a domain
(e.g. `KafkaTodoProjectionConsumerWorker`) or which constructor-injects a
domain-specific projection handler = S1. Workers MUST be generic over
`(topic, handler-resolver, schema-registry, projection-table-resolver)`.

### CHECK-PLAT-DET-01 — Determinism in adapters (S0)
grep `src/platform/host/adapters/**` for `Guid.NewGuid` / `DateTime.Now` /
`DateTime.UtcNow` / `DateTimeOffset.Now` / `DateTimeOffset.UtcNow`. Any hit
= S0. Sole permitted exception: `SystemClock` (the `IClock` implementation)
in `Program.cs`. Adapters MUST consume `IIdGenerator` (with deterministic seed
derived from aggregate id+version) and `IClock`.

### CHECK-PLAT-EVENTSTORE-SEED-01 — Postgres adapter row id (S0)
`PostgresEventStoreAdapter` row ids MUST derive from
`$"{aggregateId}:{version}"` via `IIdGenerator`. Any non-deterministic id
generation = S0.

### CHECK-PLAT-KAFKA-TIMESTAMP-01 — Projection envelope timestamps (S1)
`GenericKafkaProjectionConsumerWorker` (and any sibling) MUST stamp
`EventEnvelope.Timestamp` from injected `IClock.UtcNow`, NOT consume-moment
wall clock.

## SCORING

| Severity | Penalty |
|---|---|
| S0 violation | -40 per occurrence |
| S1 violation | -10 per occurrence |
| S2 violation | -5 per occurrence |
| S3 violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |

## NEW CHECKS INTEGRATED — 2026-04-08 (Phase 1 gate blockers)

- **CHECK-PLAT-DISPATCH-ONLY-01** (S1): Grep `src/platform/api/controllers/**` for
  `_intentHandler.HandleAsync` or any direct `IIntentHandler` invocation in action methods. Any match
  = violation. Assert every `[HttpPost|HttpPut|HttpPatch|HttpDelete]` action body invokes
  `_dispatcher.DispatchAsync` and returns `CommandResult`. DRIFT-4.
- Source: `claude/new-rules/_archives/20260408-000000-phase1-gate-blockers.md`.
