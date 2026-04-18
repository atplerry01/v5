# Infrastructure Audit (Canonical)

**Validates:** [`claude/guards/infrastructure.guard.md`](../../guards/infrastructure.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the infrastructure layer (platform + systems edge) complies with all rules consolidated in the canonical infrastructure guard: platform API boundaries, systems integration points, Kafka event fabric (topics, headers, DLQ, partitions), config/secret safety, and composition-loader / program-composition discipline at the host seam.

## Scope

- `src/platform/**` — platform APIs, host composition
- `src/platform/host/composition/` — composition loader
- `src/platform/host/Program.cs` — program composition root
- `src/systems/**` — system integrations (upstream / midstream / downstream adapters)
- `infrastructure/event-fabric/kafka/**` — Kafka topic definitions, create-topics scripts
- `infrastructure/**` and `docker-compose*.yml` — config files, secrets, environment
- All Kafka producer/consumer call sites

## Source guard

This audit checks the rules defined in [`claude/guards/infrastructure.guard.md`](../../guards/infrastructure.guard.md). Rule ID conventions (R-PLAT-01..12 with G-PLATFORM-* aliases, R-SYS-01..15 with SYS-BOUND-01/SYS-NO-STEP-01 aliases, R-K-01..25 with K-TOPIC/K-DETERMINISTIC/K-DLQ/K-HEADER aliases, R-CFG-R1..R5 + R-CFG-DC1 + R-CFG-K1 with CFG-* aliases, G-COMPLOAD-01..07, G-PROGCOMP-01..05, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Platform Boundaries

- [ ] **R-PLAT-01** (alias G-PLATFORM-01) — API layer purity
- [ ] **R-PLAT-02** (alias G-PLATFORM-02) — host layer purity
- [ ] **R-PLAT-03** (alias G-PLATFORM-03) — no controllers in host
- [ ] **R-PLAT-04** (alias G-PLATFORM-04) — no DI in API
- [ ] **R-PLAT-05** (alias G-PLATFORM-05) — API must not reference runtime or engines
- [ ] **R-PLAT-06** (alias G-PLATFORM-06) — API calls systems layer only (via `ISystemIntentDispatcher`)
- [ ] **R-PLAT-07** (alias G-PLATFORM-07) — host wires all layers *(host → domain reference is FORBIDDEN per domain guard DG-R5-HOST-DOMAIN-FORBIDDEN; legacy alias G-PLATFORM-07 no longer permits it)*
- [ ] **R-PLAT-08** (alias PLAT-NO-DOMAIN-01) — platform must not reference domain
- [ ] **R-PLAT-09** (alias PLAT-RESOLVER-01) — platform resolver constraint
- [ ] **R-PLAT-10** (alias PLAT-KAFKA-GENERIC-01) — platform Kafka generic
- [ ] **R-PLAT-11** (alias PLAT-DET-01) — platform determinism
- [ ] **R-PLAT-12** (alias PLAT-DISPATCH-ONLY-01) — dispatch-only platform

### Section 2 — Systems Integration

- [ ] **R-SYS-01** — composition only
- [ ] **R-SYS-02** — no execution logic
- [ ] **R-SYS-03** — no domain mutation
- [ ] **R-SYS-04** — no direct persistence
- [ ] **R-SYS-05** — upstream / midstream / downstream placement
- [ ] **R-SYS-06** — systems compose via runtime
- [ ] **R-SYS-07** — no domain type construction
- [ ] **R-SYS-08** — system boundary declaration
- [ ] **R-SYS-09** — no infrastructure imports inside systems
- [ ] **R-SYS-10** — idempotent composition
- [ ] **R-SYS-11** — systems must not hold state
- [ ] **R-SYS-12** — systems must not evaluate policy
- [ ] **R-SYS-13** — systems must not emit events
- [ ] **R-SYS-14** (alias SYS-BOUND-01) — `ISystemIntentDispatcher` permitted at systems entry points
- [ ] **R-SYS-15** (alias SYS-NO-STEP-01) — no step execution in systems

### Section 3 — Kafka Event Fabric

- [ ] **R-K-01** — Kafka used only through runtime
- [ ] **R-K-02** — dual-topic pattern
- [ ] **R-K-03** — topic naming convention
- [ ] **R-K-04** — no direct Kafka in domain
- [ ] **R-K-05** — no direct Kafka in engines
- [ ] **R-K-06** — outbox pattern mandatory
- [ ] **R-K-07** — consumer group naming
- [ ] **R-K-08** — dead letter topic required
- [ ] **R-K-09** — schema governance
- [ ] **R-K-10** — no Kafka configuration in domain or engines
- [ ] **R-K-11** — partition key alignment
- [ ] **R-K-12** — exactly-once semantics
- [ ] **R-K-13** — runtime outbox only
- [ ] **R-K-14** — event schema versioning required
- [ ] **R-K-15** — order guarantee by aggregate id
- [ ] **R-K-16** (alias K-TOPIC-01) — topic naming rule 1
- [ ] **R-K-17** (alias K-TOPIC-02) — topic naming rule 2
- [ ] **R-K-18** (alias K-TOPIC-CANONICAL-01) — canonical topic discipline
- [ ] **R-K-19** (alias K-DETERMINISTIC-01) — deterministic partition keys
- [ ] **R-K-20** (alias K-TOPIC-COVERAGE-01) — every event has a declared topic
- [ ] **R-K-21** (alias K-OUTBOX-ISOLATION-01) — outbox isolation
- [ ] **R-K-22** (alias K-TOPIC-DOC-CONSISTENCY-01) — docs match `create-topics.sh`
- [ ] **R-K-23** (alias K-DLQ-PUBLISH-01) — DLQ publish rules
- [ ] **R-K-24** (alias K-HEADER-CONTRACT-01) — header contract (correlation, chain, policy-decision id)
- [ ] **R-K-25** (alias K-DLQ-001) — DLQ ordering
- [ ] **R-K-26** (alias K-AGGREGATE-ID-HEADER-01) — non-empty `aggregate-id` header + key; startup probe rejects zero Guid when payload AggregateId non-empty
- [ ] **R-K-27** (alias INFRA-KAFKA-LISTENER-SYMMETRY-01) — host-machine `appsettings*.json` `Kafka:BootstrapServers` aligns with EXTERNAL listener in `docker-compose.yml`
- [ ] **R-K-28** (alias INTEGRATION-BRIDGE-OUTBOX-01) — cross-topic event-emitting bridges route through an outbox-backed aggregate OR are declared in the integration-bridge registry; direct `IProducer<>` from `src/platform/host/adapters/**` (other than `KafkaOutboxPublisher`) = violation

### Section 4 — Config & Secret Safety

- [ ] **R-CFG-R1** (alias CFG-R1) — no plaintext secrets
- [ ] **R-CFG-R2** (alias CFG-R2) — no committed credentials
- [ ] **R-CFG-R3** (alias CFG-R3) — env-var sourcing discipline
- [ ] **R-CFG-R4** (alias CFG-R4) — config source canonicalization
- [ ] **R-CFG-R5** (alias CFG-R5) — secret surface minimization
- [ ] **R-CFG-DC1** (CFG-R1 Docker Compose extension) — compose-scan globs catch secrets in `docker-compose*.yml`
- [ ] **R-CFG-K1** (alias CFG-K1) — Kafka-specific config safety (broker creds, SASL)

### Section 5 — Composition Loader

- [ ] **G-COMPLOAD-01** — registry membership
- [ ] **G-COMPLOAD-02** — explicit order
- [ ] **G-COMPLOAD-03** — locked execution sequence
- [ ] **G-COMPLOAD-04** — loader-only composition
- [ ] **G-COMPLOAD-05** — `BootstrapModuleCatalog` preserved
- [ ] **G-COMPLOAD-06** — no reflection discovery
- [ ] **G-COMPLOAD-07** — modules are orchestration-only

### Section 6 — Program Composition

- [ ] **G-PROGCOMP-01** — composition only at `Program.cs`
- [ ] **G-PROGCOMP-02** — size cap
- [ ] **G-PROGCOMP-03** — classification-aligned domain wiring
- [ ] **G-PROGCOMP-04** — no inline middleware definition
- [ ] **G-PROGCOMP-05** — locked pipeline order

### Section 7 — WBSM v3 Global Enforcement (shared)

- [ ] **GE-01** — deterministic execution
- [ ] **GE-02** — WHYCEPOLICY enforcement
- [ ] **GE-03** — WHYCECHAIN anchoring
- [ ] **GE-04** — event-first architecture
- [ ] **GE-05** — CQRS enforcement

### Section 8 — Rules Promoted from new-rules/ (2026-04-18)

- [ ] **INFRA-EVENT-FABRIC-01** — Kafka publish whitelist; direct `IProducer<>` use outside `KafkaOutboxPublisher` is sanctioned-exception-only
- [ ] **BACKFILL-PORTABILITY-01** — backfill scripts avoid locked-down extensions (`dblink`, `postgres_fdw`) or emit clear fallback message
- [ ] **OPS-VAL-001** — `kafka-init` completes before host services publish/consume; R-K-20 startup guard fails fast on mismatch
- [ ] **OPS-VAL-002** — controller parse/conversion errors return `400 BadRequest`, never unhandled `500`
- [ ] **OPS-VAL-004** — OpenAPI schema endpoint operational or alternate documented

---

## Check Procedure

1. Load the infrastructure guard rule set.
2. Execute per-section `Check Procedure` blocks in the guard.
3. Run docker-compose scan globs across `infrastructure/**` and root compose files.
4. Diff `infrastructure/event-fabric/kafka/create-topics.sh` against documented topic inventory.
5. Record verdicts with file:line evidence.

## Pass / Fail Criteria

- **PASS:** All rules `PASS` or `N/A`.
- **FAIL:** Any S0/S1 failure.
- **CONDITIONAL:** S2/S3 per $1c.

## Output Format

```
AUDIT:           infrastructure
GUARD:           claude/guards/infrastructure.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~65
SECTIONS:        8
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 and $1c.
