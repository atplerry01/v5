# Constitutional Audit (Canonical)

**Validates:** [`claude/guards/constitutional.guard.md`](../../guards/constitutional.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that every operation is bound to WHYCEPOLICY at the pre-execution gate, that every policy decision is captured/hashed/chain-anchored, that the determinism substrate (time/ID/hash/replay) holds end-to-end, and that the system invariants (INV-*) are preserved — exactly as declared in the constitutional guard.

## Scope

- All runtime entry points, command dispatchers, query handlers, projection consumers
- Policy registry, declarations, bindings
- Decision capture / chain anchoring sinks; audit trail outputs
- All ID generation, time access, hashing call sites
- Replay paths (workflow resume, projection rebuild, policy replay)
- System invariant enforcement points (command totality, persist/anchor/outbox atomicity, actor context, trust, events, workflow lifecycle, telemetry)

## Source guard

This audit checks rules defined in [`claude/guards/constitutional.guard.md`](../../guards/constitutional.guard.md). Rule ID conventions (POL-, PB-, POL-AUDIT-, DET-, G1..G20 HSID, REPLAY-, POLICY-REPLAY-INTEGRITY-, INV-, GE-) are owned by that guard.

---

## Validation Checklist

### Section 1 — WHYCEPOLICY Authority (Policy Declaration & Binding)

- [ ] **POL-01** — policies declared explicitly in registry
- [ ] **POL-02** — no unauthorized domain actions
- [ ] **POL-03** — policy enforcement through runtime
- [ ] **POL-04** — policy scope binding present
- [ ] **POL-05** — policy registry completeness
- [ ] **POL-06** — policy composition shape valid
- [ ] **POL-07** — policy versioning
- [ ] **POL-08** — temporal policies honour clock seam
- [ ] **POL-09** — escalation policies wired
- [ ] **POL-10** — separation of policy and domain rules
- [ ] **POL-11** — simulation before enforcement
- [ ] **PB-01** — policy id required at entry
- [ ] **PB-02** — policy must be resolved
- [ ] **PB-03** — policy must be active
- [ ] **PB-04** — policy scope must match execution domain
- [ ] **PB-05** — policy version must be current
- [ ] **PB-06** — policy rules must be non-empty
- [ ] **PB-07** — block-level rules must be present
- [ ] **PB-08** — policy source validation
- [ ] **PB-09** — policy → chain link required
- [ ] **PB-10** — policy context propagation

### Section 2 — Policy Evaluation Audit Trail

- [ ] **POL-AUDIT-12** — policies are auditable
- [ ] **POL-AUDIT-13** — policy violations produce domain events
- [ ] **POL-AUDIT-14** (alias **POLICY-EVENT-REQUIRED-01**) — policy evaluations produce events
- [ ] **POL-AUDIT-15** (alias **POLICY-NO-SILENT-DECISION-01**) — no silent policy decisions

### Section 3 — Determinism Substrate

- [ ] **DET-STOPWATCH-OBSERVABILITY-01** — Stopwatch permitted only for observability; never flows into hash/id/persisted payload
- [ ] **DET-SQL-NOW-ADDENDUM-01** — SQL now()/current_timestamp/getdate() not consumed by deterministic logic
- [ ] **DET-ADAPTER-01** — block-list honoured in `src/platform/host/adapters/**`; IClock / IIdGenerator used
- [ ] **DET-EXCEPTION-01** — `SystemClock` is the only permitted reader of `DateTimeOffset.UtcNow`
- [ ] **DET-SEED-01** — PostgresEventStoreAdapter row id derives from `{aggregateId}:{version}` via IIdGenerator
- [ ] **DET-SEED-DERIVATION-01** — seed material derived from declared inputs only
- [ ] **DET-IDCHECK-COVERAGE-01** — deterministic-id-check scans tests/** and scripts/validation/** in addition to src/**
- [ ] **DET-DUAL-SEAM-01** — two deterministic identity seams non-overlapping
- [ ] **DET-HSID-CALLSITE-01** — `IDeterministicIdEngine.Generate(...)` called only from runtime control-plane and T0U determinism engine

### Section 4 — Deterministic Identifiers (HSID v2.1)

- [ ] **G1** — single engine
- [ ] **G2** — no randomness
- [ ] **G3** — topology required
- [ ] **G4** — sequence bounded + last
- [ ] **G5** — domain purity
- [ ] **G6** — single stamp point
- [ ] **G7** — structural validation
- [ ] **G8** — no runtime-order mutation
- [ ] **G12** — engine required
- [ ] **G13** — sequence source
- [ ] **G14** — topology trust
- [ ] **G15** — prelude enforcement
- [ ] **G16** — sequence store
- [ ] **G17** — HSID command interface
- [ ] **G18** — sequence width
- [ ] **G19** — infrastructure readiness
- [ ] **G20** — migration required

### Section 5 — Hash Determinism

- [ ] **Hash permitted-inputs discipline** (constitutional.guard.md §Permitted Hash Inputs) — only declared, canonicalized inputs feed hash functions
- [ ] **Hash forbidden-inputs discipline** (constitutional.guard.md §Forbidden Hash Inputs) — no time, no random, no ambient identity, no mutable ordering, no floating-point drift
- [ ] **Hash required patterns** — SHA256 only; canonical serialization; explicit byte ordering
- [ ] **Hash-Specific Fail Criteria (S0)** — any drift from the hash compliance ledger halts execution

### Section 6 — Replay Determinism

- [ ] **REPLAY-SENTINEL-PROTECTED-01** — replay sentinel fields read-only to non-replay paths
- [ ] **REPLAY-SENTINEL-LIFT-01** — sentinel-lift sequence honoured before replay begins
- [ ] **REPLAY-A-vs-B-DISTINCTION-01** — A-replay (projection rebuild) vs B-replay (workflow resume) not conflated
- [ ] **POLICY-REPLAY-INTEGRITY-01** — policy decisions replay to identical verdicts; chain anchors reproduce
- [ ] **INV-REPLAY-LOSSLESS-VALUEOBJECT-01** — every VO wrapper used as a domain-event constructor parameter has a `JsonConverter<T>` registered in `EventDeserializer.StoredOptions`; parameter-name mismatches carry `[JsonPropertyName]`; every replay-loaded aggregate has a round-trip regression test

### Section 7 — System Invariants

- [ ] **INV-001** — command outcome totality
- [ ] **INV-002** — persist · anchor · outbox atomicity
- [ ] **INV-003** — no silent success
- [ ] **INV-101** — double-entry enforcement
- [ ] **INV-102** — conservation of value
- [ ] **INV-103** — settlement irreversibility
- [ ] **INV-104** — reconciliation convergence
- [ ] **INV-201** — mandatory actor context
- [ ] **INV-202** — no anonymous execution
- [ ] **INV-203** — trust score mandatory
- [ ] **INV-204** — privileged action traceability
- [ ] **INV-301** — state change ⇒ event
- [ ] **INV-302** — mandatory event metadata
- [ ] **INV-303** — persisted events are chain-anchored & replay-safe
- [ ] **INV-401** — workflow lifecycle totality
- [ ] **INV-402** — no orphan workflows
- [ ] **INV-403** — deterministic resume
- [ ] **INV-404** — step output reproducibility
- [ ] **INV-501** — mandatory telemetry emission
- [ ] **INV-502** — canonical failure reasons
- [ ] **INV-503** — infrastructure health & reason mapping
- [ ] **INV-IDEMPOTENT-LIFECYCLE-INIT-01** — layered-defense idempotency (IdempotencyMiddleware + engine-handler load-then-guard + `(aggregate_id, version)` UNIQUE); every static-factory handler injects `IEventStore`, probes, throws typed `Already<X>Recorded` `DomainException` when prior events exist; cross-host regression test present

### Section 8 — WBSM v3 Global Enforcement (shared)

- [ ] **GE-01** — deterministic execution
- [ ] **GE-02** — WHYCEPOLICY enforcement
- [ ] **GE-03** — WHYCECHAIN anchoring
- [ ] **GE-04** — event-first architecture
- [ ] **GE-05** — CQRS enforcement

---

## Check Procedure

1. Load the constitutional guard rule set.
2. For each section, execute the per-section `Check Procedure` block declared in the guard.
3. Record a verdict per rule: `PASS` / `FAIL` / `N/A` with file:line evidence.
4. Aggregate by section and overall.

## Pass / Fail Criteria

- **PASS:** All verdicts `PASS` or `N/A` with justification.
- **FAIL:** Any S0 or S1 failure.
- **CONDITIONAL:** S2/S3 captured to `claude/new-rules/` per CLAUDE.md $1c.

## Output Format

```
AUDIT:           constitutional
GUARD:           claude/guards/constitutional.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   ~70
SECTIONS:        8
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list of rule IDs>
S1_FAILURES:     <list of rule IDs>
EVIDENCE:        <path to verdict log>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12 and $1c.
