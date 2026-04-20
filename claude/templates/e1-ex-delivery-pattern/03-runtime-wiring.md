# 03 — Runtime Wiring

## What runtime owns

The runtime is the **only** layer that:

- Persists events (event store).
- Publishes events (outbox → Kafka).
- Anchors evidence (WhyceChain).
- Evaluates policy (WHYCEPOLICY middleware).
- Stamps deterministic identifiers (HSID via T0U engine).

Per `domain.guard.md` $7 layer purity. Per behavioral rule 4 (NO BYPASS OF RUNTIME CONTROL PLANE).

## Locked 8-stage middleware pipeline

Per `runtime.guard.md` runtime-order. Order is **locked** — do not reorder, do not add a 9th stage without updating the guard.

```
HSID stamp (control-plane prelude — not a middleware) →
  1. Identity context resolution
  2. Policy evaluation
  3. Authorization
  4. Idempotency claim
  5. Execution guard (deterministic preconditions)
  6. Engine dispatch
  7. Event persistence + outbox enqueue
  8. Chain anchor
```

The HSID stamp at line 1 above is the **control-plane prelude** in [src/runtime/control-plane/RuntimeControlPlane.cs](../../../src/runtime/control-plane/RuntimeControlPlane.cs), per HSID rule G6 (SINGLE STAMP POINT).

## What an EX vertical must wire

For each new vertical, register:

### 1. Command dispatch routing

Reference: [src/runtime/dispatcher/RuntimeCommandDispatcher.cs](../../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs)

- Add command-type-to-handler mapping for every command in the vertical.
- Aggregate ID candidates (extracted from command payload to identify target aggregate) must include every aggregate's primary VO type.

### 2. Domain schema registration

Reference: [src/runtime/event-fabric/DomainSchemaCatalog.cs](../../../src/runtime/event-fabric/DomainSchemaCatalog.cs)

- Add `Register{Vertical}()` method.
- Register every event type with its versioning, stored type, and inbound type per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.

### 3. Value-object JSON converters

Reference: [src/runtime/event-fabric/EventDeserializer.cs](../../../src/runtime/event-fabric/EventDeserializer.cs)

- Single-primitive wrapper VOs (e.g., `record struct AccountId(Guid Value)`) are auto-handled by `WrappedPrimitiveValueObjectConverterFactory` — no per-type converter needed.
- Multi-property VOs (e.g., `record struct Money(Amount, Currency)`) need an explicit converter if used in event payloads.
- Add converter registration to `StoredOptions.Converters` in the canonical pattern (accept both bare primitive and `{"Value":...}` envelope shape for back-compat).

### 4. Composition root registration

Reference: [src/platform/host/composition/economic/EconomicCompositionRoot.cs](../../../src/platform/host/composition/economic/EconomicCompositionRoot.cs)

- Create `{Vertical}CompositionRoot` implementing the bootstrap interface.
- Register all 60+ application + workflow modules across the vertical's BCs.
- Add `new {Vertical}CompositionRoot()` to `BootstrapModuleCatalog.All` per [src/platform/host/composition/BootstrapModuleCatalog.cs](../../../src/platform/host/composition/BootstrapModuleCatalog.cs).

### 5. Topology and topic naming

Per `infrastructure.guard.md`:

- Topics live under `infrastructure/event-fabric/kafka/topics/{classification}/{context}/{domain}/`.
- Topic naming compliance: `{classification}.{context}.{domain}.{event-type}` — lowercase, dot-separated.
- Outbox / DLQ / retry topics per [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs) pattern.

### 6. Policy registration

Per `constitutional.guard.md` POL-05 (POLICY REGISTRY):

- Add policies to the registry under `infrastructure/policy/domain/{classification}/{context}/{domain}/`.
- Every command requires a corresponding policy declaration per POL-01 / PB-01 (POLICY ID REQUIRED).
- Default behavior is deny per POL-02 (NO UNAUTHORIZED DOMAIN ACTIONS).

## What runtime MUST NOT have

- ❌ Domain-specific business logic (lives in domain layer).
- ❌ Engine-to-engine wiring (engines never reference each other).
- ❌ Direct database access outside the event-store / outbox / chain adapter abstractions.
- ❌ Authorization decisions in middleware bodies — middleware delegates to OPA / WHYCEPOLICY.
- ❌ Per-vertical conditional branches in the 8-stage pipeline. Pipeline is uniform.

## Runtime quality gate per vertical

Before declaring runtime portion of the vertical D2:

- [ ] Every command in the vertical resolves through the dispatcher.
- [ ] Every event in the vertical registers in `DomainSchemaCatalog`.
- [ ] Every wrapper-struct VO in event payloads either matches the converter factory OR has a per-type converter.
- [ ] Composition root registers all the vertical's modules and is in `BootstrapModuleCatalog.All`.
- [ ] Every command has a corresponding policy ID per PB-01.
- [ ] All Kafka topics declared and present in `infrastructure/event-fabric/kafka/topics/`.
- [ ] No `Guid.NewGuid()` / `DateTime.UtcNow` / `Random` in the vertical's runtime code per DET-ADAPTER-01.
- [ ] No call to `IDeterministicIdEngine.Generate` outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/` per HSID G6.
