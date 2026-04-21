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

- Topic manifests live at `infrastructure/event-fabric/kafka/topics/{classification}/{context}/{domain}/topics.json` — one file per BC.
- Each BC declares exactly **four** topics, not per-event:
  ```
  whyce.{classification}.{context}.{domain}.commands
  whyce.{classification}.{context}.{domain}.events
  whyce.{classification}.{context}.{domain}.retry
  whyce.{classification}.{context}.{domain}.deadletter
  ```
- Prefix `whyce.` is mandatory. Suffix is one of `commands` / `events` / `retry` / `deadletter` — **never** `.dlq`.
- Reference manifest: [infrastructure/event-fabric/kafka/topics/economic/transaction/transaction/topics.json](../../../infrastructure/event-fabric/kafka/topics/economic/transaction/topics.json).
- Outbox drain and retry/deadletter routing via [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs).

### 6. Policy registration

Policy registration covers **two distinct kinds** of policy, wired in two different places. Do not conflate them.

#### 6a. Authorization / governance policies (WHYCEPOLICY / OPA)

Per `constitutional.guard.md` POL-05 (POLICY REGISTRY):

- Add policies to the registry under `infrastructure/policy/domain/{classification}/{context}/{domain}/`.
- Every command requires a corresponding policy declaration per POL-01 / PB-01 (POLICY ID REQUIRED).
- Default behavior is deny per POL-02 (NO UNAUTHORIZED DOMAIN ACTIONS).
- Evaluated at middleware stage 2 via `IPolicyEvaluator` per [06-infrastructure-contracts.md](06-infrastructure-contracts.md) § 06.10.

#### 6b. Cross-system domain invariants (in-code domain policies)

Cross-system invariants defined in [01-domain-skeleton.md](01-domain-skeleton.md) and enforced per [02-engine-skeleton.md](02-engine-skeleton.md) are **domain-level code**, not OPA bundles. They are registered in the composition root alongside engine handlers and workflow steps.

**Registration pattern:**

```csharp
// src/platform/host/composition/{vertical}/{Vertical}CompositionRoot.cs

services.AddDomain{Vertical}Invariants();
```

with the extension registering every `{Concept}Policy` under `src/domain/{classification}-system/invariant/`:

```csharp
public static IServiceCollection AddDomain{Vertical}Invariants(this IServiceCollection services)
{
    services.AddSingleton<ContractEconomicPolicy>();
    services.AddSingleton<AmendmentContentPolicy>();
    services.AddSingleton<SettlementLedgerPolicy>();
    // one entry per policy class under src/domain/{cls}-system/invariant/
    return services;
}
```

Rules:

1. **Singletons.** Policies are pure and stateless — register as singletons.
2. **Injected by concrete type.** Handlers and workflow steps take the concrete `{Concept}Policy` class, not an interface — these are domain types, not adapters.
3. **No DI inside the policy.** Policies stay pure per `domain.guard.md` D-PURITY-01 — they do not depend on repositories, projectors, or adapters. Any required fact comes in through the `{Concept}PolicyInput` record assembled by the handler.
4. **Composition-root only.** Only `{Vertical}CompositionRoot` registers invariants; no other layer touches the DI container for policies.

#### 6c. Optional OPA externalization for cross-system invariants

A cross-system invariant MAY be externalized to OPA when the rule is operator-tunable (e.g., threshold, tenant-scoped override, regulator-driven change). The canonical path:

- Keep the in-code `{Concept}Policy` class as the **default** evaluator.
- Back it with an OPA bundle under `infrastructure/policy/domain/{classification}/invariant/{policy-name}/`.
- Inject `IPolicyEvaluator` into the `{Concept}Policy` implementation; the policy delegates to OPA and returns the typed `PolicyDecision`.
- Record the externalization in the policy's class doc and in the BC README.

Rules:

1. **Authorization/governance** (who may invoke, org rules) → **OPA by default**.
2. **Cross-system domain invariants** (is the resulting state consistent) → **in-code by default**; OPA only when operator-tunable.
3. Even when externalized, the `{Concept}Policy` in-code class remains the stable API that handlers depend on. The OPA/in-code split is an implementation detail.
4. OPA MUST NOT override domain truth per [06-infrastructure-contracts.md](06-infrastructure-contracts.md) § 06.10. A cross-system invariant externalized to OPA must still deny cases the in-code default would deny.

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
- [ ] `{Vertical}CompositionRoot` calls `AddDomain{Vertical}Invariants()` and every `{Concept}Policy` class under `src/domain/{cls}-system/invariant/` is registered exactly once.
- [ ] Every cross-system invariant referenced by a handler or workflow step resolves from DI (not `new`-ed) per 6b.
