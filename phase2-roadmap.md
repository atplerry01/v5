# Phase 2 — Economic Core & Structural Alignment

## STATUS: DESIGN — DRAFT 1
## DATE: 2026-04-10
## ENTRY GATE: Phase 1.5 LOCKED ([phase1.5-re-certification.audit.md](claude/audits/phase1.5/phase1.5-re-certification.audit.md))

---

## 0. Strict entry rules (load-bearing for every section below)

1. **Phase 1.5 runtime invariants are non-negotiable.** No Phase 2
   design decision may weaken any §3 gate of the re-certification
   audit (determinism, F1 no data loss, F2 no duplicate processing,
   multi-instance correctness, failure recovery, observability).
2. **Acceptance criteria only tighten.** Any new domain that lands in
   Phase 2 MUST pass the §5.6 fault matrix and the §5.7 observability
   surface. New domains do not inherit a free pass.
3. **Layer purity.** Every economic concept introduced below resolves
   to one of three layers and one only:
   - **Domain** — owns truth (invariants, aggregates, events).
     Zero dependencies. The economic facts live here.
   - **Policy (WHYCEPOLICY)** — owns rules that change without code
     deploys (limits, fees, eligibility, jurisdictional toggles).
   - **Runtime (Phase 1.5)** — enforces only. Persists, anchors,
     publishes, projects. **The runtime does not learn anything new
     in Phase 2.** This is the canonical guarantee that Phase 2
     economic complexity does not contaminate Phase 1.5.
4. **Design only.** This document contains no code, no scaffolding,
   no partial implementation. Promotion to implementation is a
   separate step gated by §5 below.

---

## 1. Economic Core Definition

The Phase 2 economic core is five domain concepts. Each is a pure
DDD aggregate with deterministic identifiers and replay-safe events.
Each lives under `src/domain/{classification}/{context}/{domain}/`
following the LOCKED three-level nesting rule.

### 1.1 Capital

**Purpose:** the unit of economic value the platform represents and
governs. Capital is **what** is being scaled — investor commitments,
operator equity, retained earnings, restricted reserves.

**Core invariants** (domain layer, zero deps):
- Capital is denominated. (Currency / instrument class)
- Capital is sourced. (Provenance is non-erasable)
- Capital is bounded. (No negative balance without an explicit
  authorisation event)
- Capital is classified. (Equity / debt / convertible / restricted /
  unrestricted)

**Events:**
- `CapitalCommittedEvent`
- `CapitalCalledEvent`
- `CapitalReleasedEvent`
- `CapitalReclassifiedEvent`
- `CapitalProvenanceLinkedEvent`

**What lives in policy, NOT domain:** maximum commitment per
investor, jurisdiction-specific call windows, KYC/AML eligibility
gates, fee schedules. These are WHYCEPOLICY rules, not invariants.

### 1.2 Ledger

**Purpose:** the deterministic, append-only record of every economic
movement. The ledger is **how truth is recorded** — Phase 2's
canonical source of accounting truth, sitting on top of Phase 1.5's
event store.

**Core invariants:**
- Every entry is a double-entry. (Sum of debits == sum of credits)
- Every entry references its causing economic event.
- Every entry has a deterministic id derived from the causing event.
  (No `Guid.NewGuid` — Phase 1.5 determinism rule extends here.)
- Entries are never updated, never deleted. (Append-only contract)
- Every account balance is the deterministic fold of its entries.

**Events:**
- `LedgerEntryPostedEvent`
- `LedgerAccountOpenedEvent`
- `LedgerAccountClosedEvent`
- `LedgerPeriodClosedEvent` (for accounting period boundaries)

**What lives in policy:** chart-of-accounts mapping (which economic
event posts to which accounts), revaluation rules, period-close
rules. The DOMAIN guarantees double-entry; POLICY chooses the
accounts.

### 1.3 Transaction

**Purpose:** the atomic economic action a participant takes — a
capital call, a distribution, a trade, a fee charge. A transaction
is **the cause** of one or more ledger entries.

**Core invariants:**
- Every transaction has a deterministic id.
- Every transaction has exactly one initiating party.
- Every transaction either commits fully or aborts fully. (Phase 1.5
  KC-7 outbox claim contract extends to multi-step transactions via
  the workflow engine.)
- Every transaction's ledger impact is computable from its event
  payload alone (replay-safe).

**Events:**
- `TransactionInitiatedEvent`
- `TransactionAuthorizedEvent`
- `TransactionPostedEvent`
- `TransactionAbortedEvent`
- `TransactionCompensatedEvent` (for failure-rollback paths —
  reuses the §5.2.3 TC-7 workflow timeout seam)

**What lives in policy:** authorisation chains (who must approve
what), per-party transaction limits, time-of-day windows, fee
calculations. The DOMAIN enforces atomicity; POLICY decides
eligibility.

### 1.4 Settlement

**Purpose:** the moment a transaction's economic effect becomes
**externally final**. Settlement is the bridge between the platform's
ledger and external counterparties (banks, custodians, regulators).

**Core invariants:**
- Settlement references the transaction it settles.
- Settlement carries an external reference (bank id, custodian
  reference, on-chain tx hash).
- Settlement is irreversible by the platform alone. (Reversal
  requires a counterparty event, not an internal correction.)
- Settlement timing is recorded against `IClock`, not wall-clock.

**Events:**
- `SettlementInstructedEvent`
- `SettlementConfirmedEvent`
- `SettlementFailedEvent`
- `SettlementReversedEvent` (only on confirmed external reversal)

**What lives in policy:** which counterparty handles which
instrument, retry windows, dispute escalation thresholds.

### 1.5 Revenue

**Purpose:** the deterministic recognition of economic gain accruing
to the platform and its stakeholders. Revenue is **why the system
runs** — it is what differentiates this platform from an extractive
one (per the project README's "real economic activities, not
extractive activities" framing).

**Core invariants:**
- Every revenue event references its source transaction(s).
- Revenue recognition is deterministic from transaction state.
  (No "magic" recognition — every dollar of revenue traces to a
  specific posted transaction.)
- Revenue is classified by stream (management fee, performance fee,
  carry, distribution, retained earnings).
- Revenue events are projected into both the ledger AND a separate
  revenue read model for stakeholder reporting.

**Events:**
- `RevenueRecognizedEvent`
- `RevenueAllocatedEvent`
- `RevenueDistributedEvent`
- `RevenueDeferredEvent`

**What lives in policy:** recognition method (cash vs accrual),
stakeholder allocation splits, fee structures.

### 1.6 Boundary contract between the five concepts

```
Transaction (cause)
   │
   ├── posts → Ledger entries (record)
   │
   ├── moves → Capital state (truth)
   │
   ├── instructs → Settlement (externalisation)
   │
   └── recognises → Revenue (recognition)
```

**Every Phase 2 economic event flows through this exact graph.** No
shortcuts, no back-channels. The graph is the only path from
"something happened" to "the platform's records reflect it."

---

## 2. Structural Alignment

Phase 2 introduces a topology that maps the economic core onto the
WBSM v3 cluster model already present in the project's TODO and
README sketches (`Whyce + ClusterName`, `Authority → SubCluster → SPV`,
`Vault → SPV → Asset → Revenue`).

### 2.1 Cluster model

```
                      Whyce  (root authority surface)
                         │
            ┌────────────┴────────────┐
            │                         │
        ClusterA                   ClusterB     ...   (named cluster)
            │                         │
       ┌────┴────┐                ┌───┴────┐
   Authority   Authority      Authority  Authority
       │           │              │          │
   SubCluster  SubCluster     SubCluster  SubCluster
       │           │              │          │
      SPV         SPV            SPV        SPV
       │           │              │          │
      Vault       Vault          Vault      Vault
       │           │              │          │
     Asset       Asset          Asset      Asset
       │           │              │          │
    Revenue    Revenue         Revenue    Revenue
```

**Concept-to-layer mapping:**

| Concept | Layer | Owns |
|---|---|---|
| **Whyce** | composition root + cross-cluster invariants | identity, root chain anchor, jurisdictional registry |
| **Cluster** | domain (named cluster) | the real-world initiative — a fund family, a programme, a business line |
| **Authority** | domain | the legal entity / decision-making body that owns the SubCluster |
| **SubCluster** | domain | a coherent execution unit owned by an Authority |
| **SPV** | domain | the legal vehicle that holds Assets and faces external counterparties |
| **Vault** | domain | the ring-fenced custody seam — every Asset is custodied via a Vault |
| **Asset** | domain | the productive economic instrument the platform represents |
| **Revenue** | domain (§1.5) | the economic gain recognised against a flow |

### 2.2 Authority → SubCluster → SPV mapping

The authority model is **policy-rich, domain-thin**:

- **Authority (domain):** has identity, jurisdiction, signature
  primitives, founding event. Cannot be deleted; can be transferred
  or dissolved through events.
- **SubCluster (domain):** belongs to exactly one Authority at any
  point in time. Re-parenting is an event, not an UPDATE.
- **SPV (domain):** belongs to exactly one SubCluster at any point in
  time. Holds Vaults. Can be wound down through a terminal event.

**The authority delegation rules** (who can act on behalf of whom,
with what limits, under what conditions) live in **WHYCEPOLICY**, not
the domain. This means jurisdictional reform, regulatory updates, or
operational policy changes ship as policy bundles — not as code
deploys.

### 2.3 Economic flow mapping

```
Capital  ──commitment──>  Vault ──custody──> SPV ──holds──> Asset
   │                                                          │
   │                                                          │
   │             ┌────────── productivity ──────────┐         │
   │             │                                  │         │
   ▼             ▼                                  ▼         ▼
Transaction ──> Ledger entry ──> Settlement ──> Revenue ──> Capital state
                                                              (updated)
```

The flow is closed: **Capital → Asset → Revenue → Capital**. This is
the loop that makes the platform a *scaling* platform rather than an
*extractive* one. Each arrow is one or more events. Each event is a
deterministic, replay-safe Phase 1.5 runtime artefact.

### 2.4 Tenant / partition keying

Every Phase 2 aggregate id is namespaced by `(cluster, authority,
sub_cluster)` so the §5.3.4 partitioning assumption holds: hot
aggregates stay co-located with their owning authority, and the
multi-instance refusal seams (PC-1 per-tenant, KC-6 workflow
per-tenant) have a meaningful tenant key. **The tenant key is set
in domain, consumed by runtime, and forwarded into observability.**

---

## 3. Scaling Strategy (grounded in §5.3.4)

This section binds Phase 2 design to the eight bottlenecks recorded
in [evidence/5.3/1m-readiness.evidence.md](claude/audits/phase1.5/evidence/5.3/1m-readiness.evidence.md).
Every architectural choice below is a remediation for a numbered
bottleneck.

### 3.1 Postgres sharding model (remediates §5.3.4 #2)

**Decision:** **per-cluster logical sharding by `(cluster, sub_cluster)`
pair.**

- The event store, chain store, and projections each become
  shard-aware. Each shard is a logical Postgres database; physical
  Postgres instances host one or more shards.
- Routing: a deterministic hash of `(cluster, sub_cluster)` selects
  the shard. The mapping table is a config-managed artifact, not a
  domain concern.
- Hot-aggregate contention (§5.3.4 bottleneck #3) is resolved
  because cross-shard contention is impossible: shards are disjoint
  by design.
- Migration path: §5.4 of the canonical Phase 1.5 roadmap adds the
  endurance evidence first; sharding lands AFTER §5.4 numbers
  validate the per-shard ceiling.
- The §5.2.1 PC-4 declared `Postgres.Pools.*` envelopes become
  per-shard envelopes. PC-1 raises in lockstep with PC-4 per the
  KC-1 binding rule.

**What this does NOT do:** physical sharding by aggregate hash, which
would break per-correlation chain linkage. Per-correlation linkage
remains the §5.6 scenario 7 invariant.

### 3.2 Outbox scaling model (remediates §5.3.4 #4)

**Decision:** **per-shard outbox + per-topic publisher fan-out.**

- Each Postgres shard has its own outbox table.
- Each shard's outbox is drained by N publisher workers (current:
  1; target: per-topic worker pool).
- The §5.2.2 KC-7 outbox claim contract (`SELECT FOR UPDATE SKIP
  LOCKED`) extends naturally to N publishers per shard.
- The §5.6 scenario 1 Kafka outage seam (PC-3 503 + DLQ promotion)
  is unchanged — a per-shard outbox just multiplies the seam by the
  shard count.
- Per-batch SELECT (rather than per-row round-trips) raises drain
  rate from the §5.3 burst-test measured ~750 rows/s to a target of
  10–25k rows/s per shard pair.

### 3.3 Multi-host distribution (remediates §5.3.4 #5)

**Decision:** **shard-affinity routing at the edge.**

- The nginx edge becomes shard-aware: requests carrying a tenant
  key route to a host that has affinity for that shard.
- Affinity is soft (a host without affinity will still serve the
  request, just at a higher per-dispatch cost via cross-shard
  hops). Hard affinity is a Phase 3 optimisation if §5.3 numbers
  show the soft-affinity floor is too high.
- The §5.5 multi-instance evidence trail and the §5.6 host-kill
  evidence remain canonical: any host can take any request, the
  affinity is an optimisation hint.
- Cluster size estimate from §5.3.4: 40 hosts at 25,000 RPS each
  reaches 1M RPS — a tractable cluster shape.

### 3.4 Redis cluster strategy (remediates §5.3.4 #6)

**Decision:** **Redis Cluster with key sharding by aggregate id.**

- The MI-1 distributed execution lock keys become
  `{cluster}:{aggregate_id}` — Redis Cluster's hash-tag syntax
  ensures keys for the same cluster co-locate on the same Redis
  shard.
- The §5.6 scenario 3 Redis outage seam (HC-9 + deterministic
  `execution_lock_unavailable` refusal) extends to per-shard Redis
  failure — losing one Redis shard degrades only the clusters that
  hash to it.
- Single-node Redis remains supported for development and the
  current §5.5 evidence baseline.

### 3.5 Workflow horizontal scaling (remediates §5.3.4 #8)

**Decision:** **per-tenant + per-workflow-name limits remain;
cross-host workflow state lives in the event store, not in-memory.**

- The §5.2.2 KC-6 `WorkflowAdmissionGate` per-tenant + per-workflow
  rate limiters extend per-host but consult a shared backend
  (Redis Cluster) for the cross-host counter.
- The §5.2.3 TC-7 `Workflow.PerStepTimeoutMs` and
  `Workflow.MaxExecutionMs` declared envelopes remain canonical.
- Workflow resume durability uses the existing Phase 1.5
  `WorkflowResumedEvent` fabric — no new persistence model.

### 3.6 Bottleneck-to-Phase-2-step mapping

| §5.3.4 Bottleneck | Phase 2 step that closes it |
|---|---|
| #1 PC-1 intake limiter | §3.1 + §3.3 (raise PC-1 in lockstep with PC-4 sharding) |
| #2 Postgres pool ceiling | §3.1 (per-cluster logical sharding) |
| #3 Advisory-lock contention | §3.1 (sharding eliminates cross-shard hot keys) |
| #4 Outbox drain rate | §3.2 (per-shard outbox + publisher fan-out) |
| #5 Horizontal scaling | §3.3 (shard-affinity routing) |
| #6 Redis SPOF | §3.4 (Redis Cluster) |
| #7 OPA hop | embedded in-process OPA — operational, not architectural; deferred to E10 |
| #8 KC-6 workflow saturation | §3.5 (workflow horizontal scaling) |

---

## 4. System Boundaries

This is the canonical answer to "where does Phase 1.5 end and
Phase 2 begin?" The boundary is **load-bearing** for the strict
entry rule that Phase 1.5 invariants must not change.

### 4.1 What REMAINS in Phase 1.5 runtime (frozen)

- **Runtime control plane** — `RuntimeControlPlane`, the entire
  middleware pipeline (Pre-Policy → Policy → Post-Policy →
  Execution), the dispatcher.
- **Event fabric** — event store interface, chain anchor,
  outbox contract, idempotency contract, sequence store contract.
- **Refusal seams** — every PC-*, KC-*, TC-*, KW-*, MI-*, HC-* seam.
  Phase 2 may introduce NEW seams; it MAY NOT replace, weaken, or
  bypass existing ones.
- **Determinism guarantees** — `IClock`, deterministic ids, SHA-256
  hashing, replay safety.
- **Observability surface** — `Whyce.Intake`, `Whyce.Policy`,
  `Whyce.Outbox`, `Whyce.Postgres`, `Whyce.EventStore`, `Whyce.Chain`,
  `Whyce.Workflow`, `Whyce.Projection.Consumer`. New meters in Phase 2
  are additive only.
- **SLO scaffold** — `docs/observability/slo/*` and the metric
  mapping. New SLO rows added in Phase 2 are additive only.
- **Multi-instance guarantees** — MI-1 distributed lock,
  outbox-claim contract, projection convergence semantics.
- **The §5.6 fault matrix** — every scenario test must continue to
  PASS against any Phase 2 composition.
- **The LOCKED guard** — `claude/guards/phase1.5-runtime.guard.md`
  R-RT-01..R-RT-10.

**Phase 2 work that touches any of the above is rejected at design
review unless it strictly extends.** Replacement requires a Phase 1.5
amendment, NOT a Phase 2 step.

### 4.2 What MOVES into Phase 2 domains

- **The five economic concepts** (§1) — Capital, Ledger, Transaction,
  Settlement, Revenue — as new domains under
  `src/domain/economic/{context}/{domain}/`.
- **The cluster topology** (§2) — Cluster, Authority, SubCluster,
  SPV, Vault, Asset — as new domains under
  `src/domain/structural/{context}/{domain}/`.
- **The workflow templates** that compose multiple economic events
  (e.g. capital call → ledger post → settlement instruction →
  revenue recognition) — as new T1M workflow definitions under
  `src/systems/midstream/wss/workflows/economic/`.
- **The projections** that surface economic state for read-side
  consumers — capital balances, ledger balances, settlement
  status, revenue dashboards — under
  `src/projections/economic/`.
- **The policy bundles** that govern the changeable rules (limits,
  fees, eligibility) — as WHYCEPOLICY rule sets under the
  appropriate policy package.

### 4.3 What does NOT move (anti-scope)

- **Identity / KYC / AML providers.** These are external systems;
  Phase 2 integrates via the Systems layer with WHYCEPOLICY rules,
  not by absorbing them into the domain.
- **External pricing / market data.** Same — external systems with
  domain-side adapters.
- **Tax / regulatory reporting.** Phase 3 work.
- **UI / front-end.** Phase 3 work.
- **On-chain settlement plumbing.** Settlement (§1.4) treats the
  on-chain reference as opaque metadata; the actual on-chain
  signing flow is Phase 3.

### 4.4 Anti-leakage rules

- **No domain reaches into runtime.** The runtime exposes interfaces;
  domains depend on those interfaces, not on runtime types. The
  §5.1.1 dependency graph rule (DG-R5-EXCEPT-01) extends into
  Phase 2.
- **No domain reaches into projections.** Projections consume
  events; domains never know a projection exists.
- **No projection reaches into another projection.** Read models
  are independent.
- **Policy never reaches into domain types.** Policy operates on
  event payloads and policy context, never on domain instances.

---

## 5. Implementation Plan — E1 → EX

Phase 2 is broken into **E-steps** (E for "Economic") that mirror
the Phase 1 E1..E12 cadence. Each E-step has a single named goal
and a single named acceptance gate. **No E-step starts until the
previous one's gate is signed.**

### 5.1 Step list

| Step | Name | Goal | Acceptance gate |
|---|---|---|---|
| **E1** | Capital domain bootstrap | Pure DDD aggregate + events for Capital, with deterministic ids and zero-dep build | Domain unit tests + dep-check clean |
| **E2** | Ledger domain bootstrap | Pure DDD aggregate + events for Ledger, double-entry invariant enforced in domain | Domain unit tests + double-entry property tests |
| **E3** | Transaction domain bootstrap | Atomic transaction aggregate + commit/abort/compensation events | Domain unit tests + workflow stub tests |
| **E4** | Settlement domain bootstrap | Settlement aggregate + external-reference shape + reversal contract | Domain unit tests |
| **E5** | Revenue domain bootstrap | Revenue aggregate + recognition event + classification | Domain unit tests |
| **E6** | Structural topology bootstrap | Cluster / Authority / SubCluster / SPV / Vault / Asset domains under `src/domain/structural/` | Domain unit tests + cluster-key invariant tests |
| **E7** | Economic event fabric | Wire the five economic concepts into the existing Phase 1.5 event fabric (event store, outbox, chain anchor, projections) | Integration tests against the multi-instance compose stack — every economic event passes through the canonical Phase 1.5 pipeline |
| **E8** | Capital-call vertical slice | First end-to-end vertical: Authority initiates a capital call → Transaction → Ledger entries → Settlement instruction → Revenue recognition (deferred for management fees) | Vertical slice integration test PASSES on the multi-instance stack with **every §5.6 fault scenario** also re-PASSING against the new event types |
| **E9** | Multi-cluster shard pilot | Land per-cluster logical sharding (§3.1) for E1–E8 against a new shard pair | §5.4 endurance test against real Postgres shows the per-shard ceiling matches the projection in §3.1 |
| **E10** | Outbox fan-out + embedded OPA | Land §3.2 outbox fan-out and §5.3.4 architectural upgrade #4 (embedded OPA) | §5.6 / §5.7 evidence reruns show no regression and the §5.7 measured SLOs improve in line with the §3 projections |
| **E11** | Redis Cluster + workflow horizontal scale | Land §3.4 + §3.5 | §5.5 multi-instance evidence reruns continue to PASS against Redis Cluster |
| **E12** | Phase 2 re-certification audit | Re-run the full §5.6 fault matrix and §5.7 observability surface against the Phase 2 composition | Binary PASS against every §3 gate of the Phase 1.5 re-certification audit |

### 5.2 First vertical slice definition (E8 in detail)

The first vertical slice exists to prove the design end-to-end with
the smallest meaningful economic action. It is the equivalent of the
Phase 1 Todo vertical slice but for economic events.

**Slice: Management-fee capital call.**

```
Actor: Authority
   │
   ▼
[Authority issues a CapitalCallCommand]
   │
   ▼
Pre-policy   ── PC-1 admit (per-tenant)
   │
   ▼
Policy       ── WHYCEPOLICY validates: caller is Authority, target SPV exists,
   │              call amount within configured ceiling, period is open
   ▼
Execution    ── Domain enforces: CapitalCommittedEvent (or rejects on cap),
   │              transaction id is deterministic from (authority, spv, period, sequence)
   ▼
Post-policy  ── Audit emission, chain anchor (TC-2/3 envelope intact)
   │
   ▼
Persist      ── Event store append (PC-4 envelope), outbox enqueue
   │
   ▼
Publish      ── Outbox publisher → Kafka (KC-7 contract)
   │
   ▼
Project      ── Capital projection updates (L-6 lag observable);
                Ledger projection posts the double-entry rows;
                Settlement instruction emitted to a downstream system;
                Revenue recognition deferred event posted with the policy-defined
                  recognition rule.
```

**Slice acceptance gate:**

1. The slice runs end-to-end against the multi-instance compose
   stack with both hosts up.
2. **Every §5.6 fault scenario** is re-run against the slice's new
   event types — Kafka outage, Postgres outage, Redis lock failure,
   OPA failure, chain failure, host kill, combined fault. Every
   scenario must PASS for the new event types with the SAME
   acceptance criteria (F1–F6) used in Phase 1.5.
3. **The §5.7 observability surface** must show every new event type
   incrementing every relevant `Whyce.*` instrument.
4. **The Phase 1.5 LOCKED guard** must continue to PASS unchanged.
5. The slice produces a `claude/audits/phase2/evidence/E8/...`
   evidence file in the same shape as the §5.6 / §5.7 records.

### 5.3 Promotion-to-implementation gate

This document is **DESIGN ONLY**. Promotion to E1 implementation
requires:

1. Design-review sign-off on §1 (economic core definitions).
2. Design-review sign-off on §2 (structural alignment).
3. Confirmation that every §3 scaling decision is grounded in a
   §5.3.4 bottleneck.
4. Confirmation that §4 boundaries do not weaken any Phase 1.5
   invariant.
5. A Phase 2 guard file at `claude/guards/phase2-economic.guard.md`
   that LOCKS the design rules in §0.

No code, no scaffolding, no partial implementation begins until
all five gates are met.

---

## 6. Open questions (deliberately not resolved here)

These are flagged so the design review can take them on
explicitly. None of them blocks design sign-off; all of them block
implementation.

1. **Currency / multi-currency ledger.** The §1.2 invariants assume
   a single accounting currency per Ledger. Multi-currency is a
   FX-revaluation concern that lands in policy, not domain — but the
   exact contract needs review.
2. **Period close mechanics.** `LedgerPeriodClosedEvent` is named
   in §1.2 but the close algorithm is policy-driven. Decide whether
   close itself is a workflow (T1M) or a one-shot domain event.
3. **Settlement reversal traceability.** Reversal must reference the
   original settlement; the question is whether the reversal is a
   new transaction or an annotation on the original.
4. **Revenue recognition timing.** Cash vs accrual is policy, but
   the domain needs a "recognition pending" state. The shape of
   that state is open.
5. **Cluster re-parenting.** The §2.2 model allows authority change
   via event, but the in-flight transaction implications are open.
6. **Shard boundary for cross-cluster transactions.** The §3.1
   sharding assumes intra-cluster locality. Inter-cluster transactions
   (e.g. one cluster pays a fee to another) need explicit
   cross-shard semantics.
7. **Phase 1.5 §5.4 / §5.5 / §5.7 still-NOT-STARTED rows.** These
   appear in the original [phase1.5-roadmap.md §6.0](phase1.5-roadmap.md)
   table. The Phase 1.5B re-open closed §5.2.6 / §5.3 / §5.4 SLO
   scaffold / §5.5 / §5.6 / §5.7 to PASS. The original §5.4
   (event-store endurance), §5.5.x (multi-instance scaling), §5.7.x
   (telemetry / SLOs / runbooks) rows in the canonical roadmap
   table remain. **Phase 2 E9 (sharding pilot) consumes them as
   prerequisites** — the design depends on them being measured, not
   extrapolated.

---

## 7. Statement

This document is **Phase 2 Design Draft 1** — DESIGN ONLY, no code,
no scaffolding, no partial implementation. It defines the economic
core (§1), structural alignment (§2), scaling strategy grounded in
§5.3.4 (§3), system boundaries that protect Phase 1.5 invariants
(§4), and an E1–E12 implementation plan with a first vertical slice
(§5).

The strict entry rules (§0) bind every section: Phase 1.5 runtime
invariants are non-negotiable; acceptance criteria only tighten; the
domain–policy–runtime layer split is canonical.

**Promotion to implementation requires the §5.3 gate.** Until that
gate is met, no E1 work begins.
