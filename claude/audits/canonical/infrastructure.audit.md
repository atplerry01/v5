# Infrastructure Audit (Canonical)

**Validates:** [`claude/guards/infrastructure.guard.md`](../../guards/infrastructure.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that the infrastructure layer (platform + systems edge) complies with all rules consolidated in the canonical infrastructure guard: platform API boundaries, systems integration points, Kafka event fabric (topics, headers, DLQ, partitions), and config/secret safety.

## Scope

- `src/platform/**` — platform APIs, host composition
- `src/systems/**` — system integrations, upstream/downstream/midstream adapters
- `infrastructure/event-fabric/kafka/**` — Kafka topic definitions, create-topics scripts
- `infrastructure/**` and `docker-compose*.yml` — config files, secrets, environment
- All Kafka producer/consumer call sites

## Source guard

This audit checks the rules defined in [`claude/guards/infrastructure.guard.md`](../../guards/infrastructure.guard.md). Rule families (R-PLAT-01..12, R-SYS-01..15, R-K-01..25, R-CFG-R1..R5 + R-CFG-DC1 + R-CFG-K1, GE-01..05) are owned by that guard.

---

## Validation Checklist

### Section 1 — Platform Boundaries
- [ ] **R-PLAT-01..12** — Platform layer purity: G-PLATFORM-* rules + 4 integrated new-rules + 1 Phase-1 gate blocker.
- [ ] **R-PLAT-06** — API dispatches through `ISystemIntentDispatcher` only.
- [ ] **Conflict note:** Legacy G-PLATFORM-07 (host → domain permitted) is superseded by domain guard's DG-R5-HOST-DOMAIN-FORBIDDEN — audit treats as forbidden.

### Section 2 — Systems Integration
- [ ] **R-SYS-01..15** — Systems integration constraints (13 numbered + SYS-BOUND-01 + SYS-NO-STEP-01).
- [ ] **R-SYS-09** — No infra imports inside systems.
- [ ] **R-SYS-14** — `ISystemIntentDispatcher` permitted at systems entry points.

### Section 3 — Kafka Event Fabric
- [ ] **R-K-01..15** — Core Kafka rules: only invoked through runtime; no direct domain/engine access.
- [ ] **K-TOPIC-01 / K-TOPIC-02 / K-TOPIC-CANONICAL-01** — Topic naming conventions.
- [ ] **K-DETERMINISTIC-01** — Deterministic partition keys.
- [ ] **K-TOPIC-COVERAGE-01** — Every event has a declared topic.
- [ ] **K-OUTBOX-ISOLATION-01** — Outbox isolation maintained.
- [ ] **K-TOPIC-DOC-CONSISTENCY-01** — Docs match `create-topics.sh`.
- [ ] **K-DLQ-PUBLISH-01 / K-DLQ-001** — DLQ publish rules; DLQ ordering.
- [ ] **K-HEADER-CONTRACT-01** — Header contract: required headers present (correlation, chain, policy decision id, etc.).

### Section 4 — Config & Secret Safety
- [ ] **CFG-R1..R5** — Core config safety (no plaintext secrets, no committed credentials, env-var sourcing).
- [ ] **CFG-R1 Docker Compose Extension** — Compose-scan globs catch secrets in `docker-compose*.yml`.
- [ ] **CFG-K1** — Kafka-specific config safety (broker creds, SASL config).

### Section 5 — WBSM v3 Global Enforcement (shared)
- [ ] **GE-01..GE-05** — Per shared block.

---

## Check Procedure

1. Load the infrastructure guard rule set (57 rules).
2. Execute per-section `Check Procedure` blocks (preserved from sources).
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
RULES_CHECKED:   57
SECTIONS:        5
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list>
S1_FAILURES:     <list>
EVIDENCE:        <path>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 + $1c.
