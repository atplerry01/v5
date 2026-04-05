# Systems Guard

## Purpose

Enforce that `src/systems/` is a pure composition layer. Systems wire engines together via runtime, define workflow topologies, and declare system boundaries — but they must never execute domain logic, mutate domain state, or perform direct persistence.

## Scope

All files under `src/systems/`. Applies to all system composition classes, configuration files, and wiring declarations. Evaluated at CI, code review, and architectural audit.

## Rules

1. **COMPOSITION ONLY** — Systems files must contain only composition logic: wiring engines, declaring pipelines, configuring runtime behavior, and defining system boundaries. No execution of business operations. No domain method invocations. No data transformations beyond routing metadata.

2. **NO EXECUTION LOGIC** — Systems must not contain loops that process data, conditionals that evaluate business rules, or algorithms that transform domain state. If a system file contains `for`, `while`, `foreach` over domain collections, or `if/else` on domain properties for business purposes, it is a violation.

3. **NO DOMAIN MUTATION** — Systems must never create, update, or delete domain aggregates. No calls to aggregate constructors, factory methods, `Apply()`, `Create()`, `Update()`, `Delete()`, or any method that modifies aggregate state. Domain mutation is exclusively the responsibility of T2E engines invoked through runtime.

4. **NO DIRECT PERSISTENCE** — Systems must not interact with databases, file systems, or external storage. No `DbContext`, `IRepository.Save()`, `File.Write()`, or storage SDK calls. Persistence is managed by infrastructure adapters invoked through the runtime pipeline.

5. **UPSTREAM / MIDSTREAM / DOWNSTREAM PLACEMENT** — Systems are classified by their position in the flow:
   - **Upstream**: Ingress systems that receive external input and dispatch commands to runtime. Examples: API composition, webhook receivers.
   - **Midstream**: Orchestration systems that compose multiple engine workflows via runtime. Examples: saga compositions, multi-step workflow definitions.
   - **Downstream**: Egress systems that compose output projections and external notifications. Examples: notification composers, report assembly.
   Each system must be placed in the correct category folder.

6. **SYSTEMS COMPOSE VIA RUNTIME** — All engine invocation from systems must go through runtime. Systems declare which engines to wire and how commands flow, but never call engine methods directly. Systems configure the runtime pipeline; runtime executes it.

7. **NO DOMAIN TYPE CONSTRUCTION** — Systems must not instantiate domain value objects, entities, or aggregates. Systems may reference domain types in generic type parameters (e.g., `ICommandHandler<CreateOrderCommand>`) but must not `new` domain objects.

8. **SYSTEM BOUNDARY DECLARATION** — Each system must explicitly declare its boundary: which BCs it touches, which engines it composes, and which runtime pipelines it configures. This declaration serves as the contract for architectural auditing.

9. **NO INFRASTRUCTURE IMPORTS** — Systems must not import infrastructure types directly (HTTP clients, database drivers, message brokers). Integration with external systems is done through T3I engines composed via runtime.

10. **IDEMPOTENT COMPOSITION** — System composition must be idempotent: applying the same system configuration twice must produce the same runtime pipeline. No ordering dependencies between system registrations unless explicitly declared.

11. **SYSTEMS MUST NOT HOLD STATE** — System composition classes must be completely stateless. No mutable instance fields, no caching, no accumulated state across invocations. Systems declare wiring topology and pipeline configuration — they do not maintain any runtime state. All state is held by domain aggregates (write-side) or projections (read-side), never by systems.

12. **SYSTEMS MUST NOT EVALUATE POLICY** — Systems must not evaluate, check, or enforce policies. Policy evaluation is a runtime middleware concern. Systems compose pipelines that include policy middleware, but systems themselves never inspect policy decisions, check actor permissions, or evaluate authorization rules. Policy logic in systems is a layer violation.

13. **SYSTEMS MUST NOT EMIT EVENTS** — Systems must not publish, raise, or dispatch domain events. Event emission is the exclusive responsibility of domain aggregates (raising events) and runtime (dispatching events). Systems compose the pipeline through which events flow, but they are not event producers. Any `IEventBus.Publish()` or event raising call in systems is a critical violation.

---

## WBSM v3 GLOBAL ENFORCEMENT

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Check Procedure

1. Scan `src/systems/` for domain aggregate/entity/value-object instantiation (`new Aggregate()`, `Aggregate.Create()`, etc.).
2. Scan for domain mutation method calls (`.Apply()`, `.Update()`, `.Delete()`, `.AddItem()`, etc. on domain types).
3. Scan for business-logic conditionals (`if (order.Status ==`, `switch (payment.State)`, etc.).
4. Scan for data-processing loops operating on domain collections.
5. Scan for persistence calls (`DbContext`, `SaveChanges`, `IRepository`, `File.`, `BlobClient`).
6. Scan for direct engine method invocations (non-runtime dispatched calls to engine types).
7. Verify all systems are placed in upstream/midstream/downstream folders.
8. Scan for infrastructure type imports (HTTP, DB, message broker types).
9. Verify each system file declares its BC and engine boundary.
10. Verify composition is idempotent (no mutable static registration, no order-dependent init).

## Pass Criteria

- All systems files contain only composition and wiring logic.
- Zero domain mutation calls found.
- Zero direct persistence calls found.
- Zero business-logic conditionals found.
- Zero direct engine invocations (all through runtime).
- All systems properly categorized as upstream/midstream/downstream.
- No infrastructure imports in systems layer.

## Fail Criteria

- System file mutates domain aggregate state.
- System file executes business logic (conditional on domain state).
- System file performs direct persistence.
- System file invokes engine method directly (bypassing runtime).
- System file instantiates domain types.
- System file imports infrastructure types.
- System not categorized in upstream/midstream/downstream.
- System composition is not idempotent.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Domain mutation in systems | `vault.Debit(amount)` in system file |
| **S0 — CRITICAL** | Direct persistence in systems | `_dbContext.SaveChangesAsync()` in system |
| **S1 — HIGH** | Business logic in systems | `if (invoice.IsPastDue)` in system |
| **S1 — HIGH** | Direct engine invocation | `_t2Engine.Execute(command)` in system |
| **S1 — HIGH** | Domain type construction | `new Money(100, Currency.USD)` in system |
| **S2 — MEDIUM** | Infrastructure import | `using Microsoft.Azure.ServiceBus;` |
| **S2 — MEDIUM** | Missing placement category | System not in upstream/midstream/downstream |
| **S2 — MEDIUM** | Data processing loop | `foreach (var item in orders)` with transformation |
| **S3 — LOW** | Missing boundary declaration | System without BC/engine boundary docs |
| **S3 — LOW** | Non-idempotent registration | Order-dependent system composition |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation required.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
SYSTEMS_GUARD_VIOLATION:
  system: <system name>
  placement: <upstream|midstream|downstream>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <composition-only behavior>
  actual: <detected execution/mutation>
  remediation: <fix instruction>
```
