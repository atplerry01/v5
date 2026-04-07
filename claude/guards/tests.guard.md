# Tests Guard

## Purpose

Enforce test architecture rules across the WBSM v3 system. Tests must mirror the domain structure, respect layer boundaries, cover the full execution pipeline, and ensure that simulation capabilities exist for policy and chain validation. Tests are a constitutional enforcement layer — not optional, not advisory.

## Scope

All test files across `tests/`. Applies to unit tests, integration tests, end-to-end tests, and simulation tests. Evaluated at CI, code review, and governance audit.

## Rules

1. **UNIT TESTS MUST MIRROR DOMAIN** — The unit test folder structure must mirror `src/domain/` exactly. For each bounded context at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/unit/{system}/{context}/{domain}/`. Each aggregate, entity, value object, service, and specification must have a corresponding test class following the `{ClassName}Tests` naming pattern.

2. **INTEGRATION TESTS MUST USE RUNTIME** — Integration tests must exercise the full runtime pipeline: command dispatch → middleware → engine → domain → event → projection. Integration tests must not call domain aggregates or engines directly. They must dispatch commands through the runtime command bus and verify results through the query/projection path. This validates the complete execution flow.

3. **E2E TESTS MUST COVER FULL PIPELINE** — End-to-end tests must cover the complete pipeline from platform entry point to persistence and projection. E2E tests verify: API request → platform → systems → runtime → engine → domain → event store → outbox → Kafka → projection. E2E tests use real infrastructure (or containerized equivalents) — no mocks at this level.

4. **NO INFRASTRUCTURE IN UNIT TESTS** — Unit tests must not reference any infrastructure type: no `DbContext`, no `HttpClient`, no Kafka types, no file system calls, no external service clients. Unit tests operate on pure domain logic with in-memory fakes or stubs for repository interfaces. Any infrastructure import in a unit test is a structural violation.

5. **SIMULATION TESTS MUST EXIST** — Policy simulation tests and chain verification tests must exist. Simulation tests validate:
   - Policy evaluation produces correct `DecisionHash` for known inputs
   - Chain anchoring produces valid `ChainBlock` entries
   - Policy simulation mode correctly reports what would happen without enforcing
   - Event replay produces consistent aggregate state
   Systems without simulation tests cannot pass governance audit.

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

1. Enumerate all BCs at D2 activation level and verify corresponding test folders exist in `tests/unit/`.
2. Verify each D2 aggregate has a corresponding `{Aggregate}Tests` class.
3. Scan integration test files for direct aggregate or engine instantiation (must use runtime pipeline).
4. Verify integration tests dispatch commands through `ICommandBus` or runtime mediator.
5. Verify E2E tests exercise the full platform-to-projection pipeline.
6. Scan unit test files for infrastructure imports (`DbContext`, `HttpClient`, `Confluent.Kafka`, etc.).
7. Verify simulation test files exist for policy evaluation and chain anchoring.
8. Verify simulation tests cover: DecisionHash generation, ChainBlock creation, policy simulation mode, event replay consistency.
9. Verify test naming follows `{ClassName}Tests` pattern.
10. Verify test folder structure mirrors domain structure.

## Pass Criteria

- All D2-level BCs have mirrored unit test folders.
- All aggregates, services, and specifications at D2 have test classes.
- Integration tests use runtime pipeline exclusively.
- E2E tests cover full pipeline.
- Zero infrastructure imports in unit tests.
- Simulation tests exist for policy and chain.
- Test folder structure mirrors domain structure.

## Fail Criteria

- D2-level BC without corresponding test folder.
- Aggregate without test class.
- Integration test calling domain/engine directly (bypassing runtime).
- E2E test missing pipeline stages.
- Infrastructure import in unit test.
- Missing simulation tests for policy or chain.
- Test folder structure does not mirror domain.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Integration test bypasses runtime | Test calls `aggregate.Apply()` directly instead of dispatching command |
| **S0 — CRITICAL** | Missing simulation tests | No policy simulation or chain verification tests exist |
| **S1 — HIGH** | Infrastructure in unit test | `using Microsoft.EntityFrameworkCore;` in unit test |
| **S1 — HIGH** | D2 BC without test folder | `economic-system/capital/vault/` has no test mirror |
| **S2 — MEDIUM** | Missing aggregate test | `VaultAggregate` has no `VaultAggregateTests` class |
| **S2 — MEDIUM** | E2E test missing stages | E2E test skips outbox/Kafka stage |
| **S3 — LOW** | Test naming violation | `VaultTest` instead of `VaultAggregateTests` |
| **S3 — LOW** | Test folder structure drift | Test folder doesn't match domain folder hierarchy |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation required.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
TESTS_GUARD_VIOLATION:
  test_type: <unit|integration|e2e|simulation>
  bc: <classification/context/domain>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct test structure>
  actual: <detected issue>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **T-BUILD-01**: tests/integration/ MUST compile on every CI run. A red integration project halts merge. The CI gate fails when "dotnet build tests/integration/Whycespace.Tests.Integration.csproj" fails.
- **T-DOUBLES-01**: All in-memory test doubles (InMemoryChainAnchor, InMemoryOutbox, InMemoryEventStore, etc.) MUST take IClock and IIdGenerator constructor parameters. No Guid.NewGuid() / DateTimeOffset.UtcNow inside test doubles.
- **T-PLACEHOLDER-01**: In-memory repository implementations used in production composition MUST be clearly marked as placeholders AND have a corresponding migration script in scripts/migrations/ ready for swap.
