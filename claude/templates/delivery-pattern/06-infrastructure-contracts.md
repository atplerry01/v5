# 06 — Infrastructure Contracts (E1–Ex Completion Layer)

## 06.1 Purpose

This layer completes the E1–Ex flow:

```
Domain → Engine (T2E / T1M / T0U) → Runtime → Infrastructure → API → Quality Gates
```

and it does exactly three things — no more:

- **Connects runtime to infrastructure.** Runtime is the only layer that persists, publishes, anchors, and evaluates policy per `domain.guard.md` $7 and `runtime.guard.md` runtime-order. Section 06 defines the ports through which it does so.
- **Ensures production readiness.** Delivery guarantees, retries, DLQ, idempotency, and failure isolation are specified here so every vertical (structural-system, business-system, content-system, economic-system, operational-system) operates under the same contract.
- **Preserves domain purity and replay safety.** Infrastructure is always reached via abstraction. Domain and engines never name a broker, a database, a cache, or a policy engine. The event stream plus the domain remain the sole source of truth per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.

Section 06 is contract-only: no language-specific code, no provider-specific configuration, no deployment shape.

## 06.2 Infrastructure Ports

Every infrastructure concern is expressed as a port. Domain and engines depend only on these abstractions — never on a concrete provider. The ports are **real interfaces declared in `src/shared/contracts/infrastructure/`** and consumed through composition-root wiring.

| Concern | Interface | Declaration | Key operations |
|---|---|---|---|
| Event persistence | `IEventStore` | [src/shared/contracts/infrastructure/persistence/IEventStore.cs](../../../src/shared/contracts/infrastructure/persistence/IEventStore.cs) | `AppendAsync(stream, events)`, `ReadAsync(stream, fromVersion)` |
| Outbox publish | `IOutbox` | [src/shared/contracts/infrastructure/messaging/IOutbox.cs](../../../src/shared/contracts/infrastructure/messaging/IOutbox.cs) | `EnqueueAsync(envelope)`, drain via platform worker |
| In-process dispatch | `IEventFabric` | [src/runtime/event-fabric/IEventFabric.cs](../../../src/runtime/event-fabric/IEventFabric.cs) | `DispatchAsync(envelope)` |
| Deadletter observation | `IDeadLetterStore` | [src/shared/contracts/infrastructure/messaging/IDeadLetterStore.cs](../../../src/shared/contracts/infrastructure/messaging/IDeadLetterStore.cs) | `RecordAsync(envelope, reason)`, `QueryAsync(criteria)` |
| Idempotency claim | `IIdempotencyStore` | [src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs](../../../src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs) | `TryClaimAsync(key)`, `ReleaseAsync(key)` |
| Sequence / HSID | `ISequenceStore` | [src/shared/contracts/infrastructure/persistence/ISequenceStore.cs](../../../src/shared/contracts/infrastructure/persistence/ISequenceStore.cs) | `NextAsync(sequence)` |
| Distributed lease | `IDistributedLeaseProvider` | [src/shared/contracts/infrastructure/persistence/IDistributedLeaseProvider.cs](../../../src/shared/contracts/infrastructure/persistence/IDistributedLeaseProvider.cs) | `AcquireAsync(resource, ttl)` |
| Projection persistence | `PostgresProjectionStore<T>` (concrete) + `IProjectionHandler<T>` | [src/shared/contracts/infrastructure/projection/IProjectionHandler.cs](../../../src/shared/contracts/infrastructure/projection/IProjectionHandler.cs) | `LoadAsync(id)`, `SaveAsync(model)` |
| Cache | `IRedisClient` | [src/shared/contracts/infrastructure/projection/IRedisClient.cs](../../../src/shared/contracts/infrastructure/projection/IRedisClient.cs) | `GetAsync(key)`, `SetAsync(key, value, ttl)` |
| Policy eval | `IPolicyEvaluator` | [src/shared/contracts/infrastructure/policy/IPolicyEvaluator.cs](../../../src/shared/contracts/infrastructure/policy/IPolicyEvaluator.cs) | `EvaluateAsync(policyId, input)` |

Contract rules:

1. Interfaces are declared in `src/shared/contracts/infrastructure/` and in `src/runtime/` (for runtime-owned types like `IEventFabric`). Concrete adapters live under `src/platform/host/adapters/` and `src/runtime/event-fabric/`.
2. Domain layer (`src/domain/{classification}-system/`) has zero references to any port and zero references to any adapter — per `domain.guard.md` $7 and D-PURITY-01.
3. Engines (`src/engines/T0U`, `T1M`, `T2E`) reference **only** ports they must operate through — never concrete adapters — per behavioral rules 1–4 (S0).
4. Replacing a provider (Kafka outbox → another broker, Postgres event-store → another engine, Redis → another cache, OPA → another evaluator) requires changing only the adapter registration in the composition root. No domain or engine code changes.

> **Rule:** Domain and engines must depend only on these interfaces.

## 06.3 Technology Mapping

The v1.0 canonical technology choices per `infrastructure.guard.md`:

| Interface (port) | Technology | Adapter | Responsibility |
|---|---|---|---|
| `IEventStore` | Postgres | `Whycespace.Platform.Host.Adapters.*` event-store adapter | Durable append-only event log with replay |
| `IOutbox` | Postgres → Kafka | `KafkaOutboxPublisher` | Transactional outbox drained to Kafka topics |
| `IEventFabric` | In-process | `EventDispatcher` | In-process envelope dispatch to registered handlers |
| `IDeadLetterStore` | Postgres | DLQ adapter | Observation of deadlettered envelopes |
| `IIdempotencyStore` | Postgres | Idempotency adapter | Deduplication of replayed commands |
| `ISequenceStore` | Postgres | Sequence adapter | HSID deterministic sequence source |
| `IDistributedLeaseProvider` | Postgres advisory locks | Lease adapter | Coordination across host instances |
| `PostgresProjectionStore<T>` / `IProjectionHandler<T>` | Postgres | Concrete store + per-BC handlers under `src/projections/` | Eventually-consistent read-model persistence |
| `IRedisClient` | Redis | Redis adapter | Optional, disposable key/value cache |
| `IPolicyEvaluator` | OPA | `OpaPolicyEvaluator` | Authorization + governance policy evaluation |

Constraints:

1. Concrete implementations live in the runtime layer (`src/runtime/`, `src/platform/host/adapters/`). No other layer instantiates provider clients.
2. Per-vertical topic naming follows the canonical pattern in 06.4 (Kafka) and `infrastructure.guard.md`.
3. Technology substitution is a runtime-layer concern — domain and engine code MUST NOT mention `Kafka`, `Postgres`, `Redis`, or `OPA` by name.

> **Rule:** The runtime / host-adapters layer provides the concrete implementations. No other layer may.

## 06.4 Event Delivery Guarantees

The canonical delivery contract across all five systems:

- **At-least-once delivery.** Every emitted event will be observed by every registered handler at least once.
- **Possible duplication.** Duplicates are expected under broker retries, consumer restarts, and outbox re-drain.
- **No global ordering guarantee.** Ordering is guaranteed only within a single partition key (typically aggregate ID). Cross-aggregate ordering is not promised.

Rules on handlers:

1. Handlers MUST be idempotent per 06.5.
2. No handler may assume single delivery.
3. No handler may rely on cross-aggregate ordering; causality must be captured inside domain events, not inferred from delivery order.
4. Handlers that must reconcile across aggregates obtain ordering from the event stream replay, not from the bus.

## 06.5 Idempotency Model

Idempotency is mandatory, uniform, and expressed at two points:

- **Command idempotency** is enforced at runtime middleware stage 4 (idempotency claim) per `runtime.guard.md` runtime-order. Every command carries an idempotency key; replays of the same key after commit are no-ops.
- **Event-handler idempotency** is enforced at each handler. Handlers dedupe on `(eventId, handlerId)` before applying side effects.

Rules:

1. Idempotency keys are deterministic — derived from command contents per `constitutional.guard.md` GE-01, never `Guid.NewGuid()`.
2. Deduplication state is held by the runtime/handler, never by the domain.
3. Replaying the full event stream from scratch against a cold projection or cache MUST produce the same final state as any prior replay per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.
4. A handler that cannot be made idempotent is not admissible — it must be redesigned.

## 06.6 Retry and Failure Handling

Every infrastructure-bound operation (publish, subscribe, projection write, cache write, policy evaluate) follows one retry posture:

- **Exponential backoff** with bounded attempts and jitter. Base, cap, and max-attempts are declared in configuration — never hard-coded in domain or engine code.
- **Deadletter topic per BC.** Events exceeding retry budget land in `whyce.{classification}.{context}.{domain}.deadletter` and are observed, not discarded. (Name is `deadletter` — not `dlq`.)
- **Retry topic per BC.** Intermediate retry attempts land in `whyce.{classification}.{context}.{domain}.retry` before escalation to deadletter.
- **Failure isolation per handler.** One handler's failure MUST NOT prevent other handlers from processing the same event.

Rules:

1. Retry logic lives in the runtime / adapter layer. Engines and domain never retry.
2. DLQ inspection and replay are operator workflows exposed through the platform, not through domain code.
3. Poison-message handling (schema mismatch, unknown event type) is an operator concern; the handler rejects and advances.

> **Rule:** Failures must not corrupt domain state.

## 06.7 Projection Model (Postgres)

Projections under `src/projections/{classification}/` are read models produced by applying `*EventSchema` envelopes to `PostgresProjectionStore<{Domain}ReadModel>` via per-BC handlers that implement `IEnvelopeProjectionHandler`. Their contract:

1. **Eventually consistent** with the event stream — a read may lag the latest write by some finite, observable interval.
2. **Non-authoritative.** The domain + event stream is truth; projections are derived.
3. **Rebuildable from scratch.** Dropping and re-applying every event for a vertical MUST reproduce the current projection state exactly.
4. **Pure reducers.** Projection reducers are pure `(state, event) → state` functions per 04-api-projection-tests.md and behavioral rule 9 — no I/O, no async, no `DateTime.UtcNow`.
5. **Stable schema.** Read models have typed columns; untyped `JsonElement` blobs are a smell and gated by quality review.

Projection rebuild is the primary disaster-recovery mechanism — if a projection diverges, it is discarded and rebuilt, never hand-patched.

## 06.8 Cache Model (Redis)

Cache is an accelerator, never a record.

1. **Optional and disposable.** Every read path MUST function correctly with the cache empty or unavailable.
2. **Never a source of truth.** No invariant, no authorization decision, and no domain conclusion may depend on cached data.
3. **Explicit invalidation.** Each cache key declares its invalidation trigger: TTL, event-driven (listen to events and evict), or write-through. Implicit / lazy invalidation is forbidden.
4. **No cross-vertical cache sharing** without an explicit contract — keys are namespaced by `{classification}:{context}:{domain}:...` to prevent accidental coupling.

## 06.9 Content-System Integration

Domain references content (documents, media, assets) through `DocumentRef`-style value objects — never by fetching, storing, or reasoning about content bytes directly.

Contract:

1. `DocumentRef` resolution (bytes, URL, metadata) happens in the infrastructure / content adapter, not in the domain or engine.
2. Domain MUST NOT fetch content directly, MUST NOT call `HttpClient`, and MUST NOT embed content storage paths.
3. Missing, unreadable, or revoked content MUST NOT corrupt domain invariants. The domain operates on the reference; resolution failures surface at the adapter or projection layer.
4. Content lifecycle (upload, version, redact, delete) flows through the content-system vertical's own commands and events — other verticals observe content events and react, never mutate.

## 06.10 Policy Integration (PolicyEvaluator / OPA)

The `IPolicyEvaluator` port (OPA by default — any implementation satisfies the contract) handles **authorization** and **governance**. It is distinct from both aggregate invariants AND cross-system domain invariants.

### Three kinds of policy, three homes

| Kind | Question it answers | Where it lives | Evaluated by | Evaluated where |
|---|---|---|---|---|
| **Aggregate invariants** | Is this aggregate's internal state consistent? | Aggregate + specifications under `src/domain/{cls}-system/{ctx}/{dom}/` | The aggregate itself | Inside aggregate methods |
| **Cross-system domain invariants** | Is the resulting state across contexts consistent? | `{Concept}Policy` classes under `src/domain/{cls}-system/invariant/{policy-name}/` — see [01-domain-skeleton.md](01-domain-skeleton.md) + [02-engine-skeleton.md](02-engine-skeleton.md) | In-code domain policy (optionally backed by OPA) | Engine (T2E handler or T1M step) before aggregate mutation |
| **Authorization / governance** | Who may invoke this command? What org rules apply? | OPA bundles under `infrastructure/policy/domain/{cls}/{ctx}/{dom}/` | `IPolicyEvaluator` (OPA) | Runtime middleware stage 2 |

### Contract

1. Runtime middleware stage 2 (policy evaluation) delegates to `IPolicyEvaluator` per `runtime.guard.md` runtime-order **for authorization / governance**. No engine and no domain calls the policy evaluator directly for those concerns.
2. **Authorization** (who may invoke this command?) and **governance** (cross-cutting organizational rules) live in OPA bundles under `infrastructure/policy/domain/{classification}/{context}/{domain}/` per `constitutional.guard.md` POL-05.
3. **Aggregate invariants** (what must be true of a single aggregate after this command?) live in the domain layer. OPA does not enforce them and MUST NOT override them.
4. **Cross-system domain invariants** (what must be true across contexts?) are **in-code by default** and live under `src/domain/{classification}-system/invariant/`. They MAY be backed by OPA (see 06.10a) but the in-code `{Concept}Policy` class remains the handler-facing API.
5. Default deny per POL-02 / POL-01 — a command without a registered policy ID is rejected at middleware, never reaches the engine.

### 06.10a Code policies vs OPA policies — when to use each

| Concern | In-code domain policy (`{Concept}Policy` class) | OPA bundle |
|---|---|---|
| **Cross-system domain invariants** | **Default.** Pure, versioned with the domain, testable as unit-pure functions, replay-safe, no network call. | Use **only** when the rule is operator-tunable (threshold, tenant override, regulator-driven change). Bundle path: `infrastructure/policy/domain/{cls}/invariant/{policy-name}/`. |
| **Authorization (who)** | Never. | **Default.** Always OPA. |
| **Governance (org rules)** | Never. | **Default.** Always OPA. |
| **Static business rules** (constants, enum-like whitelists) | **Default.** In-code. | Only if rules must change without a deploy. |
| **Policy that must survive replay without network dependency** | **Required.** In-code. | Forbidden — OPA is a live-evaluation dependency; replay must not depend on OPA availability. |

### 06.10b Externalizing a cross-system invariant to OPA

When a cross-system invariant is externalized:

1. The in-code `{Concept}Policy` class remains. It now **delegates** to `IPolicyEvaluator` internally and translates the evaluator's response into a typed `PolicyDecision`.
2. The OPA bundle lives under `infrastructure/policy/domain/{classification}/invariant/{policy-name}/`.
3. The policy's class doc records: the OPA bundle path, the input schema, the decision schema, and the **fallback rule** when OPA is unreachable.
4. **Fallback rule is mandatory.** An unreachable OPA during command dispatch MUST NOT silently allow a cross-system mutation — the fallback is either (a) deny, or (b) the in-code default — declared per policy.
5. Even when externalized, the invariant MUST hold on replay. During replay no OPA call is made; the historical event is trusted as evidence that the invariant held at emission time.

> **Rule:** OPA evaluates authorization and governance. Domain owns truth. OPA must not override domain truth. A cross-system invariant backed by OPA is still a domain invariant — OPA is an implementation detail of how it is expressed.

## 06.11 Runtime Responsibilities

Runtime is the wiring layer for all of section 06. For each vertical, runtime:

1. Wires concrete adapters (`KafkaEventBus`, `PostgresReadModelStore`, `RedisCacheStore`, `OpaPolicyEvaluator`) to their ports in the composition root per [src/platform/host/composition/BootstrapModuleCatalog.cs](../../../src/platform/host/composition/BootstrapModuleCatalog.cs).
2. Routes events through Kafka — declares topics, partitions, consumer groups, and outbox-drain workers.
3. Manages retries, backoff, and DLQ per 06.6.
4. Coordinates projection handlers (Postgres) — event subscription, reducer invocation, read-model persistence.
5. Manages cache (Redis) — TTL, eviction, invalidation listeners.
6. Evaluates policy (OPA) via `IPolicyEvaluator` at middleware stage 2.

Runtime MUST NOT:

- Encode domain-specific business logic (belongs in domain).
- Inline provider SDKs into engines or domain.
- Add per-vertical branches into the locked 8-stage pipeline — the pipeline is uniform across E1–Ex.

## 06.12 Cross-System Integrity Guarantees

Infrastructure must preserve the cross-system invariants established by the domain:

- **structural-system ↔ economic-system** — structural references used by economic flows (accounts, ledgers, positions) must be valid and consistent **before** the economic event is emitted. Validation happens in-engine against the projection or domain; infrastructure does not relax this check.
- **business-system ↔ economic-system** — every business-level fact (order, invoice, contract) that has a monetary consequence must carry an attribution to an economic subject. Events without attribution are rejected at the emitting engine.
- **content-system ↔ business-system / structural-system** — a content artifact's ownership (who produced it, who owns it, who may reference it) must resolve to a valid business or structural subject. Dangling content references are rejected at creation time.
- **operational-system** observes all of the above — it does not weaken them. Operational events (audits, alerts, health signals) are derived facts, never authoritative.

> **Rule:** Infrastructure must not weaken these guarantees. Retries, DLQ, cache, and projection rebuilds never bypass cross-system invariants.

## 06.13 Quality Gates (Infrastructure Layer)

Runs as part of the post-execution audit sweep per CLAUDE.md $1b. Every vertical passes all gates before its infrastructure portion is considered D2.

| # | Check | Audit approach |
|---|---|---|
| 06.13.1 | Every event handler is idempotent | Manual review — each handler declares and dedupes on `(eventId, handlerId)` |
| 06.13.2 | Zero domain dependency on infrastructure | `grep -rEn 'Kafka\|Postgres\|Redis\|OPA\|HttpClient\|DbContext\|IConnectionMultiplexer' src/domain/{cls}-system` returns empty |
| 06.13.3 | Zero engine dependency on concrete adapters | `grep -rEn 'Kafka\|NpgsqlConnection\|IConnectionMultiplexer\|OpaClient' src/engines/` returns empty |
| 06.13.4 | Projections rebuild successfully from an empty store | Replay-rebuild test produces identical `PostgresProjectionStore<{Domain}ReadModel>` state per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01` |
| 06.13.5 | Event replay produces identical state | Deterministic replay regression test per 05-quality-gates.md Gate 7.4 |
| 06.13.6 | Each BC declares the four canonical topics (`commands`, `events`, `retry`, `deadletter`) with `whyce.` prefix | `cat infrastructure/event-fabric/kafka/topics/{cls}/{ctx}/{dom}/topics.json` lists exactly the four `whyce.{cls}.{ctx}.{dom}.{commands\|events\|retry\|deadletter}` topics |
| 06.13.7 | Every command has a registered policy ID | Per POL-01 / PB-01 — cross-reference commands against `infrastructure/policy/domain/{cls}/{ctx}/{dom}/` |
| 06.13.8 | Cache has explicit invalidation for every key namespace | Manual review — every cached key declares TTL or event-driven eviction |
| 06.13.9 | No handler relies on single delivery or global ordering | Manual review — delivery contract assumptions are explicit per 06.4 |
| 06.13.10 | Every cross-system invariant externalized to OPA declares a fallback rule (deny or in-code default) per 06.10b | Manual review — OPA-backed `{Concept}Policy` class doc records the fallback; replay tests confirm no OPA dependency |
| 06.13.11 | Replay reproduces state without calling `IPolicyEvaluator` — cross-system invariants are evidence at emission time, not re-evaluated during replay | Replay regression test passes with a null / throwing `IPolicyEvaluator` stub |

## 06.14 Success Criteria

The infrastructure contracts layer is satisfied when all of the following hold for every vertical (structural-system, business-system, content-system, economic-system, operational-system):

1. **Domain remains infrastructure-agnostic.** No domain file names any provider or port.
2. **Infrastructure is replaceable.** Any single port (`IEventStore`, `IOutbox`, `IEventFabric`, `PostgresProjectionStore<T>`, `IRedisClient`, `IPolicyEvaluator`, and the supporting `IIdempotencyStore` / `ISequenceStore` / `IDistributedLeaseProvider` / `IDeadLetterStore`) can be swapped by changing only the composition-root registration.
3. **System is replay-safe.** Replaying the full event stream reproduces projections, caches, and derived state exactly.
4. **System tolerates duplication and failure.** Duplicate events, handler failures, retries, and DLQ paths do not corrupt domain state or cross-system invariants.
5. **No truth exists outside domain + event stream.** Projections are derived, caches are disposable, policy decisions are recorded, and the authoritative record is always the event stream applied through the domain.

When these hold, the template set 01–06 fully supports a vertical's path from domain modeling through production operation without compromising layer purity or replay safety.
