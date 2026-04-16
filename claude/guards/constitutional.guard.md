---
name: constitutional
type: canonical
severity: S0
locked: true
consolidates:
  - policy.guard.md
  - policy-binding.guard.md
  - determinism.guard.md
  - deterministic-id.guard.md
  - hash-determinism.guard.md
  - replay-determinism.guard.md
---

# Constitutional Guard (WBSM v3 | CANONICAL | VERSION 1.0)

This is the canonical constitutional guard of the Whycespace repository. It consolidates the governance foundations (WHYCEPOLICY authority, determinism, deterministic identity, hash determinism, replay determinism) into a single enforcement artifact. Every rule in the source guards is preserved verbatim in intent; overlapping rules are merged into their strongest, most explicit form.

## Purpose

Enforce the constitutional invariants of the platform:

- WHYCEPOLICY authorizes every state mutation and every execution context.
- All identifiers, timestamps, and hashes are deterministic and replay-stable.
- Compact external correlation identifiers (HSID v2.1) originate from a single engine via a single stamp point.
- Replay semantics (re-execution vs projection rebuild) are preserved and never silently "fixed".

Any violation halts execution at the guard-load stage per CLAUDE.md $1a and blocks merge per $16.

## Scope

- All Claude execution contexts (policy binding is the first pipeline gate).
- All code under `src/domain/**`, `src/engines/**`, `src/runtime/**`, `src/systems/**`, and `src/platform/host/adapters/**`.
- Policy declaration files, command handlers, runtime middleware, engine dispatchers, chain anchoring.
- `src/runtime/deterministic/*Hash.cs`, `src/runtime/event-fabric/EventReplay*.cs`.
- `src/engines/T0U/determinism/**`, `src/shared/kernel/determinism/**`, `src/runtime/control-plane/**`.
- Policy registry, policy evaluation middleware, and all audit trails.

---

## WHYCEPOLICY Authority

### Policy Declaration and Binding

**POL-01 — POLICIES DECLARED EXPLICITLY.** Every domain action that mutates aggregate state must have a corresponding policy declaration. Policies are defined as named artifacts with explicit conditions, authorized actors, and scope. No implicit or convention-based authorization — every mutation path must trace to a declared policy.

**POL-02 — NO UNAUTHORIZED DOMAIN ACTIONS.** A command handler must not execute a domain mutation unless the command's associated policy has been evaluated and satisfied. If no policy is bound to a command, the command must be rejected at the runtime pipeline before reaching the engine. Default behavior is deny.

**POL-03 — POLICY ENFORCEMENT THROUGH RUNTIME.** Policy evaluation occurs in the runtime middleware pipeline, before command dispatch to engines. Runtime checks policy satisfaction as a pipeline step. Engines must not evaluate policies — they assume the command has already been authorized by the time it arrives.

**POL-04 — POLICY SCOPE BINDING.** Each policy must declare its scope: which bounded context(s), aggregate(s), and command(s) it applies to. A policy without scope binding is invalid. Scope can be broad (all commands in a BC) or narrow (single command on single aggregate).

**POL-05 — POLICY REGISTRY.** All active policies must be registered in a policy registry (file or configuration). The registry serves as the canonical list of all policies in the system. Unregistered policies are not enforceable. The registry is the source of truth for governance audits.

**POL-06 — POLICY COMPOSITION.** Policies can be composed using AND/OR/NOT operators. A compound policy is satisfied only when its sub-policies evaluate correctly. Policy composition must be declarative (not imperative if/else chains). Composite policies are first-class artifacts.

**POL-07 — POLICY VERSIONING.** Policies must be versioned. When a policy changes, the previous version is retained for audit history. Active policy version is explicitly declared. No silent policy mutation — changes produce `PolicyUpdatedEvent`.

**POL-08 — TEMPORAL POLICIES.** Policies may have effective dates (start/end). A temporal policy is only active during its declared window. Expired policies are automatically deactivated. Temporal evaluation uses the injected time provider (not system clock directly).

**POL-09 — ESCALATION POLICIES.** Certain high-impact actions require escalation policies with multi-party approval. Escalation policies declare the required approval chain and quorum. No single-actor policy may authorize a high-impact action unless explicitly permitted by governance configuration.

**POL-10 — SEPARATION OF POLICY AND DOMAIN RULES.** Policies govern authorization and permission (who can do what, under what conditions). Domain specifications govern business invariants (what is valid state). These are distinct concerns. A domain specification must not check authorization; a policy must not enforce business invariants.

**POL-11 — SIMULATION BEFORE ENFORCEMENT.** Policy changes must support simulation mode before enforcement. A new or modified policy must be evaluable in `simulate` mode where it logs what would happen without actually blocking actions. Simulation results are recorded for review. Only after simulation validation should the policy be promoted to `enforce` mode.

### Pre-Execution Policy Binding (runs FIRST in the pipeline)

```
Policy Binding -> Prompt Integrity -> Guard Checks -> Execution -> Audit -> Trace
```

**PB-01 — POLICY ID REQUIRED.** Every execution context MUST include a `policyId` field. Check: `context.policyId` is present and non-empty. Severity S0-CRITICAL. Fail action: BLOCK execution immediately.

**PB-02 — POLICY MUST BE RESOLVED.** The `policyId` must resolve to a known policy (built-in or custom). Check: policy exists in DEFAULT_POLICIES or custom policies directory. Severity S0-CRITICAL. Fail action: BLOCK execution immediately.

**PB-03 — POLICY MUST BE ACTIVE.** Resolved policy must have `active: true`. Check: `policy.active === true`. Severity S0-CRITICAL. Fail action: BLOCK execution immediately.

**PB-04 — POLICY SCOPE MUST MATCH EXECUTION DOMAIN.** If the execution targets a specific domain/layer, the policy scope must include it. Check: target layer is present in `policy.scope[]`. Severity S0-CRITICAL. Fail action: BLOCK execution immediately.

**PB-05 — POLICY VERSION MUST BE CURRENT.** Policy version must match or exceed the minimum required version for WBSM v3. Check: `policy.version >= "3.0.0"`. Severity S1-HIGH. Fail action: BLOCK execution, warn operator.

**PB-06 — POLICY RULES MUST BE NON-EMPTY.** A valid policy must declare at least one enforcement rule. Check: `policy.rules.length > 0`. Severity S1-HIGH. Fail action: BLOCK execution.

**PB-07 — BLOCK-LEVEL RULES MUST BE PRESENT.** For structural and behavioral policies, at least one BLOCK-level rule is required. Check: `policy.rules.some(r => r.enforcement === "BLOCK")`. Severity S2-MEDIUM. Fail action: WARN, allow execution with advisory.

**PB-08 — POLICY SOURCE VALIDATION.** Policy MUST originate from an external authoritative source. Hardcoded or inline policy definitions are FORBIDDEN. Check: `policySource` is defined and not null; policy is resolved from external file (`registry/policies.json`) or API (OPA / WhycePolicy), never from runtime memory or hardcoded constants. Severity S0-CRITICAL. Fail action: BLOCK execution immediately. FAIL IF: `policySource === null` or `undefined`; policy resolved from hardcoded `DEFAULT_POLICIES` or inline object literals; policy resolved from runtime memory without external backing.

**PB-09 — POLICY -> CHAIN LINK REQUIRED.** Every policy decision must be anchored to the WhyceChain immutable ledger. The `DecisionHash` produced by policy evaluation must be recorded as a `ChainBlock` entry before execution proceeds. Decisions without chain anchoring are not governance-compliant and must be rejected. Check: `chainBlock.decisionHash === policyDecision.hash` and `chainBlock` is persisted. Severity S0-CRITICAL.

**PB-10 — POLICY CONTEXT PROPAGATION.** The policy decision context must flow through the entire runtime pipeline. Once a policy is evaluated and bound, its decision (including `policyId`, `version`, `decisionHash`, `result`, and `actor`) must be available to all downstream middleware, the engine dispatcher, and the chain anchoring step. Context loss at any pipeline stage is a critical violation. Check: `pipelineContext.policyDecision` is present and complete at every middleware stage. Severity S0-CRITICAL.

### Policy Evaluation Audit Trail

**POL-AUDIT-12 — POLICIES ARE AUDITABLE.** All policy evaluations (pass and fail) must be recorded in an audit log. The audit trail includes: policy name, action attempted, actor identity, evaluation result (pass/fail), timestamp, and correlation ID. This enables forensic analysis and compliance reporting.

**POL-AUDIT-13 — POLICY VIOLATIONS PRODUCE DOMAIN EVENTS.** When a policy check fails (unauthorized action attempted), the system must produce a domain event recording the violation: `PolicyViolationDetectedEvent`. This event includes the attempted action, the policy that was violated, the actor, and the timestamp. Violations are never silently swallowed.

**POL-AUDIT-14 (POL-AUDIT-01 / POLICY-EVENT-REQUIRED-01, S0).** After every WHYCEPOLICY evaluation (allow OR deny), the runtime pipeline MUST emit a domain event (`PolicyEvaluatedEvent` or `PolicyDeniedEvent`) carrying `DecisionHash`, `IdentityId`, `PolicyName`, `IsAllowed`, `CorrelationId`, and `CausationId`. A `DecisionHash` without a corresponding event is a governance violation. Decision must be independently auditable as an event, not only via chain `DecisionHash`.

**POL-AUDIT-15 (POLICY-NO-SILENT-DECISION-01, S0).** A policy evaluation that produces a `DecisionHash` but does not appear as a domain event in EventStore is forbidden. Audit trail must be queryable via the event stream, not only via chain anchors.

**POL-AUDIT-16 (P-EVT-001, S2).** Every row in the `events` table that resulted from a command path MUST have non-null `policy_decision_hash` and `policy_version`. The chain anchor carrying the hash is necessary but not sufficient — per WBSM v3 $11 the audit trail lives on the event itself. Audit query: `SELECT count(*) FROM events WHERE policy_decision_hash IS NULL` MUST equal 0 in any environment that has executed at least one command. The dispatcher pipeline / engine event-emit path is responsible for stamping these columns at write time, not a separate projection.

### Policy Decision Hashing and Chain Anchoring

**POL-HASH-17 — POLICY DECISION HASHING.** Every policy evaluation must produce a `DecisionHash` — a cryptographic hash of the policy decision including: policy ID, version, action, actor, evaluation result, and timestamp. The hash serves as a tamper-proof record of the decision. Decisions without hashes are not auditable and are therefore invalid.

**POL-HASH-18 — CHAIN ANCHOR REQUIRED FOR DECISION.** Every policy decision must be anchored to the WhyceChain immutable ledger via a `ChainBlock`. The chain block links the `DecisionHash` to the ledger, creating an immutable audit trail. Policy decisions that are not chain-anchored cannot be verified retroactively and are governance violations.

---

## Determinism

This section consolidates and extends the WBSM v3 determinism rules previously scattered across `behavioral.guard.md` (rule 16, GE-01), `domain.guard.md` (GE-01), `engine.guard.md` (GE-01), and the runtime C# guard `src/runtime/guards/DeterminismGuard.cs`.

Its distinct contribution: it **extends the determinism block list to the platform adapter surface** (`src/platform/host/adapters/**`). Adapters are the persistence and event-fabric boundary; non-determinism here breaks event-sourcing replay guarantees, idempotency, deduplication, and chain anchoring even when domain/engine/runtime code is perfectly deterministic.

### Scope (in-scope paths)

- `src/domain/**`
- `src/engines/**`
- `src/runtime/**` (additionally checked at runtime by `src/runtime/guards/DeterminismGuard.cs`)
- `src/platform/host/adapters/**` — added by this guard
- `src/systems/**`

### Block List (FORBIDDEN in all in-scope paths)

- `Guid.NewGuid()`
- `Guid.NewGuid().ToString(...)`
- `DateTime.Now`
- `DateTime.UtcNow`
- `DateTimeOffset.Now`
- `DateTimeOffset.UtcNow`
- `Random` instantiation, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)` for non-cryptographic identity/sequence generation
- `Environment.TickCount`, `Environment.TickCount64`
- `Stopwatch.GetTimestamp()` / `Stopwatch.GetElapsedTime()` used as an identity, event-stamp, or hash-input source

### Required Replacements

- For identity: `IIdGenerator.Generate(seed)` from `src/shared/kernel/domain/IIdGenerator.cs`. The seed MUST be derived deterministically from the operation's coordinates — for example `$"{aggregateId}:{version}"` for an event store row id, or `$"{commandId}:{handlerName}"` for a command-derived child id. Random or wall-clock seeds defeat the purpose.
- For time: `IClock.UtcNow` from `src/shared/kernel/domain/IClock.cs`.

Both seams are DI-registered as singletons in `src/platform/host/Program.cs` (`SystemClock` -> `IClock`, `DeterministicIdGenerator` -> `IIdGenerator`). There is no excuse for a constructor to be missing them.

### Exceptions (the only permitted boundary readers)

1. **The `IClock` implementation itself.** `SystemClock.UtcNow` in `src/platform/host/Program.cs` is the single permitted reader of `DateTimeOffset.UtcNow`. No other class.
2. **The `IIdGenerator` implementation itself.** Currently `DeterministicIdGenerator` in `src/platform/host/Program.cs`, which derives ids via `SHA256(seed)` and never reads the system clock or RNG. If a future implementation needs randomness, it must be confined to this single class.
3. **Stopwatch for observability instrumentation** (DET-STOPWATCH-OBSERVABILITY-01, S2). `Stopwatch.GetTimestamp()` / `Stopwatch.GetElapsedTime()` are PERMITTED solely for observability instrumentation (latency histograms, counters, traces). The resulting value MUST NOT flow into `ExecutionHash`, deterministic IDs, sequence seeds, chain block IDs, or any persisted event payload. Lint: any data flow from `Stopwatch` to a hash/id constructor is a DET violation.

### SQL Clock Exception (DET-SQL-NOW-ADDENDUM-01, S3)

SQL `NOW()` / `CURRENT_TIMESTAMP` inside SQL statements is permitted **only** for storage-layer operational timestamps (`created_at`, `projected_at`, `published_at`, `next_retry_at`) in infrastructure adapter SQL, provided:

- The timestamp is never consumed by domain logic, event replay, chain integrity, or deterministic ID generation.
- Business-significant time is always supplied from the `IClock` seam via parameterized values.
- The usage is confined to the platform/host adapter layer (never in domain or engine SQL).

If any new SQL timestamp is introduced that could affect replay correctness or business logic, it must use a parameterized timestamp from `IClock` instead of `NOW()`. A SQL-clock value that flows back into an aggregate, projection key, event hash, or chain anchor is a violation.

### Adapter-Specific Rules

**DET-ADAPTER-01.** Block list extended to `src/platform/host/adapters/**`. Forbidden: `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`. Use `IIdGenerator.Generate(seed)` with deterministic seed derived from aggregate id/version/stream coordinate, and `IClock.UtcNow`.

**DET-EXCEPTION-01.** The `IClock` implementation (`SystemClock`) is the ONLY permitted reader of `DateTimeOffset.UtcNow`. SQL `NOW()` / `CURRENT_TIMESTAMP` is permitted ONLY for audit columns the application does NOT read back into deterministic logic.

**DET-SEED-01.** `PostgresEventStoreAdapter` row id MUST derive from `"{aggregateId}:{version}"` via `IIdGenerator`. Kafka projection envelopes MUST stamp `Timestamp` from `IClock.UtcNow`, not consume-moment wall clock.

**DET-SEED-DERIVATION-01 (S1).** When invoking `IIdGenerator.Generate(seed)` (or any seam producing a deterministic identifier from a seed string), the seed MUST be composed exclusively of stable command coordinates (aggregate id, command type name, aggregate version, correlation/causation id, deterministic discriminators). FORBIDDEN seed components: `IClock.UtcNow`/`DateTime.*`/`Stopwatch.*`/`Ticks`, `Guid.NewGuid()`/`Random.*`/`RandomNumberGenerator.*`, process/thread/machine identifiers, env vars, or hashes thereof. Static check: search `IIdGenerator.Generate(` and flag any seed-string interpolation containing `Clock|Now|Ticks|Guid|Random`. Architecture-test enforcement under `tests/unit/architecture/WbsmArchitectureTests`. Rationale: non-deterministic seeds defeat the entire deterministic-id mechanism and silently break replay, projection idempotency, and chain integrity.

**DET-IDCHECK-COVERAGE-01 (S2).** `scripts/deterministic-id-check.sh` (or a sibling script) MUST scan `tests/**` and `scripts/validation/**` in addition to `src/**`. Test paths and validation harnesses are not exempt from determinism rules.

**DET-DUAL-SEAM-01 (S1).** TWO deterministic identity seams are canonical with non-overlapping responsibilities:

| Seam | Output | Scope |
|------|--------|-------|
| `IIdGenerator.Generate(seed)` | `Guid` | Internal row ids, hash inputs, adapter envelopes. Sole implementation: `DeterministicIdGenerator` (SHA256 of seed -> Guid). |
| `IDeterministicIdEngine.Generate(...)` | compact `string` `PPP-LLLL-TTT-TOPOLOGY-SEQ` | External-facing correlation IDs. Sole implementation: `Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. |

Both must remain free of `Guid.NewGuid`, `DateTime*.UtcNow`, `Random`, `Environment.Tick*`.

**DET-HSID-CALLSITE-01 (S1).** `IDeterministicIdEngine.Generate(...)` MUST NOT be called outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/`.

---

## Deterministic Identifiers (HSID v2.1)

This section locks the HSID v2.1 compact correlation-id format and its single source of truth. This is the **second** deterministic identity guard alongside the determinism block list above; the two are intentionally non-overlapping per DET-DUAL-SEAM-01.

### Locked Format

```
PPP-LLLL-TTT-TOPOLOGY-SEQ
```

| Segment | Width | Charset | Meaning |
|---------|-------|---------|---------|
| PPP | 3 | `[A-Z]` | `IdPrefix` enum name |
| LLLL | 4 | `[A-Z]` | `LocationCode` |
| TTT | 3 | `[A-Z0-9]` | Deterministic time bucket (SHA256 of seed) |
| TOPOLOGY | 12 | `[A-Z0-9]` | Cluster (3) + SubCluster (3) + SPV (6) |
| SEQ | 3 | `[A-Z0-9]` | Bounded sequence (`X3`, 0..0xFFF) |

Canonical regex:

```
^[A-Z]{3}-[A-Z]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$
```

### Rules

**G1 — SINGLE ENGINE.** All HSIDs MUST be produced by `Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. No other class may construct an HSID literal or implement `IDeterministicIdEngine`.

**G2 — NO RANDOMNESS.** The engine, the bucket provider, and the sequence resolver MUST NOT call `Guid.NewGuid`, `Random*`, `RandomNumberGenerator`, `DateTime*.UtcNow`, `DateTimeOffset*.UtcNow`, `Environment.Tick*`, or `Stopwatch.GetTimestamp`. The bucket is derived from the seed via SHA256.

**G3 — TOPOLOGY REQUIRED.** Every HSID MUST encode a `TopologyCode` of exactly 12 characters: Cluster (3) + SubCluster (3) + SPV (6). A topology value with any other width is a violation.

**G4 — SEQUENCE BOUNDED + LAST.** The sequence segment MUST be the LAST segment, MUST be exactly 3 hex chars (`X3`), and MUST be produced by an `ISequenceResolver` whose scope key includes BOTH the topology and the seed. Unbounded counters or sequences keyed only on time are forbidden.

**G5 — DOMAIN PURITY.** No code under `src/domain/**` may inject or call `IDeterministicIdEngine`. The domain layer is forbidden from naming its own HSIDs.

**G6 — SINGLE STAMP POINT.** `IDeterministicIdEngine.Generate(...)` may be called from EXACTLY two surfaces:
1. `src/runtime/control-plane/RuntimeControlPlane.cs` (the prelude that stamps `CommandContext.Hsid`).
2. `src/engines/T0U/determinism/**` (the engine itself, for self-tests).

A call from any other path is an architectural violation. The HSID is stamped before the locked 8-middleware pipeline runs and is write-once on `CommandContext.Hsid`.

**G7 — STRUCTURAL VALIDATION.** Every produced HSID MUST pass `IDeterministicIdEngine.IsValid(id)` immediately after generation. The prelude in `RuntimeControlPlane` performs this check; new call sites MUST do the same.

**G8 — NO RUNTIME-ORDER MUTATION.** This guard MUST NOT be used as a justification to add a 9th middleware to the locked pipeline in `runtime-order.guard.md`. The HSID stamp lives in the control-plane prelude, out of band of the 8-stage pipeline.

### H2–H6 Hardening Additions (locked 2026-04-07)

**G12 — ENGINE REQUIRED.** `IDeterministicIdEngine`, `ISequenceResolver`, and `ITopologyResolver` MUST all be configured. The `RuntimeControlPlane` constructor throws on any null. There is no fallback, no nullable injection, no optional DI. NO ENGINE -> NO EXECUTION.

**G13 — SEQUENCE SOURCE.** The canonical sequence resolver is `PersistedSequenceResolver` backed by `ISequenceStore`. The previous `InMemorySequenceResolver` has been removed. Reintroducing an in-memory resolver in production composition is a violation; the test `InMemorySequenceStore` is permitted ONLY under `tests/integration/setup/`.

**G14 — TOPOLOGY TRUST.** Topology MUST come from `ITopologyResolver` (via `IStructureRegistry`) for any command that implements `IHsidCommand`. Caller-supplied topology in a command body, request DTO, or HTTP header is forbidden. The fallback path (non-`IHsidCommand`) derives topology deterministically from `classification|context|domain` via SHA256 — this fallback is permitted but flagged by audit A14.

**G15 — PRELUDE ENFORCEMENT.** HSID stamping MUST occur in the `RuntimeControlPlane` prelude, before the locked 8-middleware pipeline runs. No middleware may stamp or replace `CommandContext.Hsid`. The write-once setter on `CommandContext.Hsid` enforces this at runtime.

**G16 — SEQUENCE STORE.** `ISequenceStore` MUST exist as a dedicated persistence contract. `IEventStore` MUST NOT be used for HSID sequence persistence. Cross-using the event store for sequence counters conflates two replay surfaces.

**G17 — HSID COMMAND INTERFACE.** Commands MAY implement `IHsidCommand`. If implemented, `RuntimeControlPlane` MUST resolve topology via `ITopologyResolver`. A command that implements `IHsidCommand` but bypasses the resolver is a G14 violation.

**G18 — SEQUENCE WIDTH.** Sequence segment MUST remain X3 (3 hex chars, 0..0xFFF). Any change to width MUST update this guard, the engine regex, and the audit regex in the SAME commit.

**G19 — INFRASTRUCTURE READINESS.** `ISequenceStore.HealthCheckAsync()` MUST be invoked at host bootstrap by `HsidInfrastructureValidator`. The runtime MUST NOT begin accepting traffic if the health check returns false or throws. Silent degradation is forbidden.

**G20 — MIGRATION REQUIRED.** The `hsid_sequences` table MUST exist in every environment that runs the host. Its canonical migration lives at `infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql`. CI MUST run `scripts/hsid-infra-check.sh` against the target database before any deploy.

VIOLATION = BLOCKER.

---

## Hash Determinism

Lock the inputs to `ExecutionHash` and `DecisionHash` to a deterministic, replay-stable set. Any change that introduces a timestamp, RNG value, unordered collection, or non-normalized field into a hash input is a critical violation. Hash determinism is the foundation of replay verification — if hashes drift, replay cannot prove anything.

### Scope

- `src/runtime/deterministic/ExecutionHash.cs`
- `src/runtime/deterministic/DeterministicHasher.cs` (if present)
- Any future `*Hash.cs` file under `src/runtime/deterministic/`
- The `DecisionHash` field/computation in policy evaluation (`src/engines/T0U/whycepolicy/` and the policy result type)

### Permitted Hash Inputs

Only the following classes of input may feed a hash:

1. **Stable identifiers** — `correlationId`, `commandId`, `aggregateId`, `policyId`, `identityId`, `tenantId`. These are deterministic per command and reproduced exactly on replay.
2. **Normalized identity context** — `roles` (joined in canonical sort order), `trustScore` (string-formatted with invariant culture), `policyVersion` (string).
3. **Policy decision artifacts** — `policyDecisionHash`, `policyDecisionAllowed` (as a stable string).
4. **Domain event content** — event type names + per-event payload hashes computed via `DeterministicHasher.ComputePayloadHash(...)`. Events must be hashed in their emission order, with the position index included so that two events of the same type at different positions produce different signatures.
5. **Counts** — `domainEvents.Count` as a string. Counts are derived from the event list and are themselves deterministic.

### Forbidden Hash Inputs (S0 — CRITICAL)

The following are FORBIDDEN as direct or indirect inputs to any hash computed in scope:

- `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow` — wall clock readings vary per run.
- `IClock.UtcNow` — even though `IClock` is the canonical time seam, its value is not stable across replays. Time may be hashed by the *event payload* (because events carry their own deterministic stamps), but the hash function itself must not call into `IClock`.
- `Guid.NewGuid()` — non-deterministic by definition.
- `Random`, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)` for hash salt — defeats determinism.
- `Environment.TickCount`, `Environment.TickCount64`, `Stopwatch.GetTimestamp()`.
- **Unordered collections** as inputs — `HashSet<T>`, `Dictionary<K,V>`, `ConcurrentDictionary<K,V>`, or any `IEnumerable<T>` that is not explicitly sorted before hashing. Iteration order of unordered collections is implementation-defined and may vary across runs and framework versions.
- **Non-normalized strings** — locale-sensitive `ToString()` calls (`(0.5).ToString()` rather than `(0.5).ToString(CultureInfo.InvariantCulture)`), case-sensitive comparisons of canonical identifiers without first applying a documented canonicalization, paths with mixed separators.
- **Reference equality** — hashing the result of `Object.GetHashCode()` or `RuntimeHelpers.GetHashCode()` couples the hash to memory layout.
- **Floating-point representations** — directly hashing `double`/`float` bit patterns. Use the canonical decimal string form via `InvariantCulture` round-trip format ("R" / "G17") only when absolutely necessary, and prefer `decimal` for any value that flows into a hash.

### Required Patterns

1. **Composite hashes** must be computed via `DeterministicHasher.ComputeCompositeHash(...)` (or equivalent). The composite function must concatenate inputs with a reserved separator that cannot appear in the inputs, so that `("ab", "c")` and `("a", "bc")` produce different hashes.
2. **Per-event signatures** must include the event's positional index, not just its type and payload. Two `TodoUpdatedEvent`s with the same payload at positions 0 and 1 must produce different per-event signatures.
3. **Sort before hash.** When hashing a collection, sort by a stable key first. The sort must be culture-invariant and document the comparator.
4. **Null sentinels.** When a field may be null, hash a fixed sentinel string (e.g. `"none"`, `"anonymous"`) rather than letting the null propagate. The sentinel must be unique enough that it cannot collide with a real value.

### Current Compliance (capture date 2026-04-07)

`ExecutionHash.cs:23-61` is currently **compliant**. Inputs:

- `correlationId.ToString()`, `commandId.ToString()`, `aggregateId.ToString()` — stable ids
- `identityId ?? "anonymous"`, `roles` joined, `trustScore?.ToString() ?? "0"` — normalized identity
- `policyId`, `policyDecisionHash ?? "none"`, `policyDecisionAllowed?.ToString() ?? "false"`, `policyVersion ?? "none"` — policy artifacts with sentinels
- `eventSignatures` built as `$"{type}:{i}:{payloadHash}"` per event — position index included
- `domainEvents.Count.ToString()` — derived count

**Zero forbidden inputs.** No clock read, no RNG, no unordered collection, no locale-sensitive formatting. The file passes this guard by inspection.

### Hash-Specific Fail Criteria (S0)

- Any forbidden input reaches a hash function in scope.
- Per-event signatures lack position indexing.
- An unordered collection is hashed without an explicit sort.

Hash drift is silent and breaks replay verification without producing an error at the point of mistake. Block merge until remediated; the matching `replay-determinism.audit.output.md` must re-PASS.

---

## Replay Determinism

Lock the design intent behind `EventReplayService.ReplayAsync` and protect the sentinel envelope fields it produces during projection rebuild. This section exists to prevent future passes from "fixing" sentinels that are intentional design markers.

### Scope

- `src/runtime/event-fabric/EventReplayService.cs`
- Any future `EventReplay*.cs` file under `src/runtime/event-fabric/`
- The audit document `claude/audits/replay-determinism.audit.output.md` which records the by-design rationale and is the source of truth for this section.

### Background: Two Notions of Replay

- **Type A — Re-execution.** Run the same commands twice through the full RuntimeControlPlane -> Engine -> EventFabric pipeline. With a frozen `IClock` and the existing `DeterministicIdGenerator`, every envelope field including `ExecutionHash`, `PolicyHash`, and `Timestamp` is byte-equal between runs. This is the property protected by the **Hash Determinism** section above.

- **Type B — Projection rebuild.** Use `EventReplayService.ReplayAsync` to load events from the event store and dispatch them to projection handlers. This path **deliberately** sets sentinel values:
  - `PolicyHash = "replay"`
  - `ExecutionHash = "replay"`
  - `Timestamp = DateTimeOffset.MinValue`

  The sentinels signal to downstream consumers that the envelope is a rebuild artifact, not a fresh execution. They are a feature, not a bug.

### Rules

**REPLAY-SENTINEL-PROTECTED-01 — Sentinels are protected design artifacts (S1).** The three sentinel assignments in `EventReplayService.ReplayAsync` MUST remain in place. Any code change, prompt instruction, or audit finding that proposes replacing them with "real" envelope values is a violation of this guard and MUST be rejected at the guard-load stage.

The protected statements are:

```csharp
ExecutionHash = "replay",
PolicyHash    = "replay",
Timestamp     = DateTimeOffset.MinValue,
```

Located at `EventReplayService.cs:55-59`.

**REPLAY-SENTINEL-LIFT-01 — How to lift the protection (S1).** The protection is **not absolute**, but lifting it requires a documented design change, not a hardening fix. The path to changing the sentinel behavior is:

1. **First** update `claude/audits/replay-determinism.audit.output.md` to remove the by-design clause at lines 53-72 and record the new requirement that justifies the change. Without this update, no downstream change is permitted.
2. Extend `EventStoreService` (or its successor) to persist and return per-event envelope metadata (`PolicyHash`, `ExecutionHash`, `Timestamp`) at the time the events are appended to the store.
3. Modify `EventReplayService.ReplayAsync` to read those values from the store rather than reconstructing envelopes from raw events.

Steps 2 and 3 may not be performed in any commit that does not also contain step 1.

**REPLAY-A-vs-B-DISTINCTION-01 — Audits and prompts must respect the distinction (S2).** Any audit, prompt, or test that asserts envelope-field equality on replay MUST distinguish Type A (re-execution) from Type B (rebuild):

- For Type A: `ExecutionHash`, `PolicyHash`, and `Timestamp` MUST be byte-equal between runs under a frozen `IClock` and deterministic `IIdGenerator`. Failure here is a true determinism violation under the Hash Determinism / Determinism sections above.
- For Type B: `ExecutionHash`, `PolicyHash`, and `Timestamp` are sentinel values and MUST NOT be asserted equal to the originals. The fields that DO survive rebuild are `EventId`, `AggregateId`, `CorrelationId`, `EventType`, `Payload`, and `SequenceNumber`.

Asserting Type A equality on a Type B replay is a misclassification and a violation of this rule.

**POLICY-REPLAY-INTEGRITY-01 (S0).** `EventReplayService` MUST NOT re-evaluate policy during replay. Stored `PolicyEvaluatedEvent` / `PolicyDeniedEvent` records are the source of truth for replayed decisions. Re-evaluation would risk drift if policy versions or trust scores have changed since original evaluation.

**INV-REPLAY-LOSSLESS-VALUEOBJECT-01 — Event-Sourcing Round-Trip Losslessness For Value Objects (S1).** The aggregate state reconstructed from the event store on replay MUST equal the state that produced those events at write time (modulo intentional sentinels documented under REPLAY-SENTINEL-PROTECTED-01). Any silent data loss in the `write → JSONB → read → aggregate` round trip is a violation.

Every domain event whose constructor parameters include a value-object wrapper struct (e.g. `record struct VaultAccountId(Guid Value)`, `record struct Currency(string Code)`) MUST round-trip through `PostgresEventStoreAdapter` + `EventDeserializer.DeserializeStored` losslessly. Concretely:

- Every value-object type that appears as a domain-event constructor parameter MUST have a corresponding `JsonConverter<T>` registered in `EventDeserializer.StoredOptions` (`src/runtime/event-fabric/EventDeserializer.cs`). The converter MUST accept BOTH the raw primitive form (the schema-mapped shape persisted to JSONB) and the legacy `{"Value": ...}` envelope form (back-compat with prior writes).
- Every domain event whose constructor parameter name differs from the schema's JSON key MUST carry a `[property: JsonPropertyName("<schema-key>")]` attribute on the parameter, so STJ binds the JSON key to the constructor parameter on replay.
- Every aggregate type that is replay-loaded MUST have a regression test that (1) constructs the aggregate via its factory, (2) schema-maps the seed event the same way `OutboxAdapter`/`PostgresEventStoreAdapter` does, (3) serializes to JSONB, (4) deserializes via `EventDeserializer.DeserializeStored`, (5) replays via `LoadFromHistory`, (6) asserts every value-object field on the aggregate equals its pre-write value. The vault regression test `tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs` is the canonical template.

Static check: enumerate every aggregate under `src/domain/**/aggregate/` and every domain event referenced by its `Apply` method; for each constructor parameter whose CLR type is `record struct`, confirm a converter is registered in `EventDeserializer.StoredOptions`. Each missing converter is an S1 fail. Failure mode is silent — no exception at write time, no exception on apply; the aggregate happily sets `Currency = default(Currency)` and the mismatch surfaces only when downstream code reads the post-apply state.

Complements REPLAY-SENTINEL-PROTECTED-01: sentinels are explicit, documented exceptions; silent value-object data loss is the failure this rule prevents.
**Source:** new-rules 2026-04-16 (revenue-domain validation D10).

---

## WBSM v3 GLOBAL ENFORCEMENT (Shared)

These five global enforcement clauses appear in the source guards and are canonical here.

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` / `IClock` for temporal operations

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

- Write model != Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Shared Check Procedure

### Policy Authority checks

1. Verify policy source is defined and external (PB-08).
2. Extract `policyId` from execution context (PB-01).
3. Resolve policy from external source (PB-02).
4. Verify `active` status (PB-03).
5. Verify scope coverage against target domain (PB-04).
6. Verify version meets minimum (PB-05).
7. Verify rules are non-empty (PB-06).
8. Verify BLOCK-level rules exist for structural/behavioral policies (PB-07).
9. Enumerate all command types across the system.
10. For each command, verify a policy binding exists in the policy registry.
11. Scan runtime middleware pipeline for policy evaluation step — must be present before command dispatch.
12. Verify policy evaluation middleware produces `PolicyViolationDetectedEvent` on failure.
13. Verify audit log captures all policy evaluations (pass and fail).
14. Scan engine code for policy evaluation logic — must be zero (engines assume pre-authorized).
15. Verify all policies in the registry have explicit scope declarations.
16. Verify policy versioning — check for version field and `PolicyUpdatedEvent` on changes.
17. Verify domain specifications do not check authorization/actor identity.
18. Verify policies do not enforce domain business invariants (state validity).
19. Scan for commands that bypass runtime pipeline (direct handler invocation without policy check).
20. Verify temporal policies use injected time provider.

### Determinism checks

1. Ripgrep the union of `src/domain`, `src/engines`, `src/runtime`, `src/systems`, and `src/platform/host/adapters` for each blocked pattern.
2. For every hit, classify:
   - Is the file the `IClock` or `IIdGenerator` implementation? -> exception.
   - Is the hit inside a comment or string literal? -> not a violation.
   - Otherwise -> S0 violation.
3. For each adapter constructor, verify it accepts `IClock` and/or `IIdGenerator` whenever it stamps timestamps or generates ids.

### HSID checks

1. Ripgrep `src/` for `Guid.NewGuid`, `DateTime*.UtcNow`, `Random`, `Environment.Tick*`, `Stopwatch.GetTimestamp` inside `src/engines/T0U/determinism/**` and `src/shared/kernel/determinism/**`. Any hit -> S0.
2. Ripgrep `src/` for `IDeterministicIdEngine` references. Any reference outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/` -> S1 (G6).
3. Ripgrep `src/domain/` for `IDeterministicIdEngine` or `HSID`. Any hit -> S0 (G5).
4. Open `RuntimeControlPlaneBuilder.Build()` and confirm the locked list still contains exactly 8 middlewares (G8 cross-check vs `runtime-order.guard.md`).
5. Confirm the regex in `DeterministicIdEngine.Format` matches the canonical regex in this document character-for-character.

### Hash checks

1. Open every file in scope. Read the body of every `Compute*` / `Hash*` method.
2. For every input expression, classify as permitted or forbidden by the lists above.
3. Grep the scope for `DateTime`, `Guid.NewGuid`, `Random`, `HashSet<`, `Dictionary<`, `Environment.Tick`, `Stopwatch`. Each hit must be either absent from the hash inputs or justified by a documented sentinel pattern.
4. Verify per-event signatures include the position index when iterating a list of events.

### Replay checks

1. Open `EventReplayService.cs`. Verify the three sentinel assignments are present, in the order shown above, with the exact literal values (`"replay"`, `"replay"`, `DateTimeOffset.MinValue`).
2. Grep `src/runtime/event-fabric/EventReplay*.cs` for any code that reads `ExecutionHash`, `PolicyHash`, or `Timestamp` from a stored event metadata source — if present, confirm `replay-determinism.audit.output.md` no longer contains the by-design clause (i.e. step 1 of the lift procedure has been completed). If the audit still has the clause, the read is a violation of REPLAY-SENTINEL-LIFT-01.
3. Grep `tests/**` and `claude/audits/**` for assertions on `ExecutionHash` or `PolicyHash` equality across replays. For each hit, classify as Type A or Type B per REPLAY-A-vs-B-DISTINCTION-01.

---

## Shared Pass / Fail / Severity

### Pass Criteria (combined)

- Every command has a bound policy in the registry; policy evaluation occurs in runtime middleware for all commands.
- Policy violations produce auditable domain events; all policy evaluations (pass/fail) are audit-logged.
- No policy evaluation logic in engines; all policies have explicit scope declarations; policies are versioned with change events; clear separation between policies (authorization) and specifications (invariants).
- All S0 and S1 policy-binding rules pass and policy context is successfully bound to execution.
- Zero determinism block-list hits in in-scope paths after exception filtering; every adapter that stamps a timestamp injects `IClock`; every adapter that generates a row id injects `IIdGenerator` and uses a deterministic seed.
- All HSIDs come through the single engine via the single prelude; engine, bucket, and sequence are clock- and RNG-free; topology is always 12 chars; sequence is always 3 hex chars; domain layer is HSID-blind; runtime middleware pipeline is still the locked 8-stage list.
- Every hash input is in the permitted list or is a documented sentinel; no forbidden inputs reach a hash, directly or transitively; per-event signatures are position-aware; composite hashes use a reserved separator.
- All three replay sentinel assignments present and unchanged; no code in `src/runtime/event-fabric/` reads stored envelope metadata for `ExecutionHash` / `PolicyHash` / `Timestamp` unless the lift procedure has been completed; all replay equality assertions correctly distinguish A from B.

### Fail Criteria (combined)

- Command without bound policy in registry; missing policy evaluation step in runtime middleware; policy violation silently swallowed (no event, no log); policy evaluation logic in engine code; policy without scope declaration; policy mutation without versioning or change event; policy enforcing domain business invariant; domain specification checking authorization; command bypassing policy evaluation pipeline.
- ANY S0 policy-binding rule fails -> immediate BLOCK; ANY S1 rule fails -> BLOCK with diagnostic; S2 rules -> WARN only.
- Any blocked determinism pattern in an in-scope path outside the two permitted exception files; an adapter that constructs an envelope/row with `DateTimeOffset.UtcNow` or `Guid.NewGuid()` inline; an `IIdGenerator.Generate(seed)` call where the seed is `Guid.NewGuid()`, `DateTimeOffset.UtcNow.Ticks`, or any other non-derived value.
- Random / clock primitive in `src/engines/T0U/determinism/**` or `src/shared/kernel/determinism/**`; domain-layer code references `IDeterministicIdEngine`; sequence segment width changed without updating both this guard and the engine regex; `IDeterministicIdEngine.Generate(...)` called outside the two permitted surfaces; a second `IDeterministicIdEngine` implementation added; topology derivation collapses to fewer than 12 chars.
- Any forbidden input reaches a hash function in scope; per-event signatures lack position indexing; an unordered collection is hashed without an explicit sort.
- Any replay sentinel assignment removed or changed without the lift procedure complete; new code reading stored metadata for the protected fields without the audit update; Type B replay assertions claiming envelope equality on the protected fields (S2).

### Severity Matrix

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Command without bound policy | `TransferFundsCommand` has no policy binding |
| **S0 — CRITICAL** | Policy violation silently swallowed | Failed auth check with no event or log |
| **S0 — CRITICAL** | Command bypasses policy pipeline | Direct handler invocation without auth |
| **S0 — CRITICAL** | PB-01..PB-04, PB-08, PB-09, PB-10 failure | Missing/unresolved/inactive/unscoped policy; hardcoded policy source; missing chain anchor; lost context propagation |
| **S0 — CRITICAL** | Direct `Guid.NewGuid()` or `DateTime*.UtcNow` in domain, engine, runtime, systems, or platform adapter | Inline `DateTimeOffset.UtcNow` in adapter |
| **S0 — CRITICAL** | `IIdGenerator.Generate(seed)` called with a non-deterministic seed | Seed contains `Guid.NewGuid().ToString()` |
| **S0 — CRITICAL** | Random / clock primitive in `src/engines/T0U/determinism/**` or `src/shared/kernel/determinism/**` | |
| **S0 — CRITICAL** | Domain-layer code references `IDeterministicIdEngine` | |
| **S0 — CRITICAL** | Sequence segment width changed without updating both this guard and the engine regex | |
| **S0 — CRITICAL** | Any forbidden input reaches a hash function in scope | Hashing `DateTime.UtcNow` into `ExecutionHash` |
| **S0 — CRITICAL** | Per-event signatures lack position indexing | |
| **S0 — CRITICAL** | An unordered collection is hashed without an explicit sort | |
| **S0 — CRITICAL** | `EventReplayService` re-evaluates policy during replay | POLICY-REPLAY-INTEGRITY-01 |
| **S0 — CRITICAL** | Policy `DecisionHash` not emitted as domain event | POL-AUDIT-14 / 15 |
| **S1 — HIGH** | Policy evaluation in engine | Engine checking `if (actor.HasRole("admin"))` |
| **S1 — HIGH** | Missing audit trail | Policy evaluations not logged |
| **S1 — HIGH** | Policy without scope | Policy declared but not bound to any command/BC |
| **S1 — HIGH** | PB-05 / PB-06 failure | Policy version below 3.0.0; empty `rules[]` |
| **S1 — HIGH** | SQL clock value flowing back into a hash, key, or anchor | |
| **S1 — HIGH** | `IDeterministicIdEngine.Generate(...)` called outside the two permitted surfaces | G6 / DET-HSID-CALLSITE-01 |
| **S1 — HIGH** | A second `IDeterministicIdEngine` implementation is added | |
| **S1 — HIGH** | DET-SEED-DERIVATION-01 — non-stable seed component | |
| **S1 — HIGH** | Replay sentinel assignment removed or changed without the lift procedure | REPLAY-SENTINEL-PROTECTED-01 / LIFT-01 |
| **S2 — MEDIUM** | Unversioned policy | Policy changed without version increment |
| **S2 — MEDIUM** | Specification checking authorization | `OrderValidSpec` checking actor permissions |
| **S2 — MEDIUM** | Policy enforcing invariant | Policy checking `if (balance >= amount)` |
| **S2 — MEDIUM** | PB-07 failure | Structural policy has no BLOCK-level rule |
| **S2 — MEDIUM** | P-EVT-001 — `events.policy_decision_hash` or `policy_version` NULL | |
| **S2 — MEDIUM** | New adapter added without `IClock` / `IIdGenerator` injection where it stamps or ids | |
| **S2 — MEDIUM** | Topology derivation collapses to fewer than 12 chars | |
| **S2 — MEDIUM** | DET-STOPWATCH-OBSERVABILITY-01 — Stopwatch flowing into a hash/id | |
| **S2 — MEDIUM** | DET-IDCHECK-COVERAGE-01 — tests/validation not scanned | |
| **S2 — MEDIUM** | Type B replay assertion claiming envelope equality on protected fields | REPLAY-A-vs-B-DISTINCTION-01 |
| **S3 — LOW** | Missing temporal bounds | Policy without effective dates where applicable |
| **S3 — LOW** | Unregistered policy | Policy class exists but not in registry |
| **S3 — LOW** | DET-SQL-NOW-ADDENDUM-01 boundary drift | SQL `NOW()` flowing into non-operational column |

---

## Shared Enforcement Action

- **S0**: Block merge. Fail CI. Phase 2 lock condition fails. Mandatory remediation before any further review.
- **S1**: Block merge. Fail CI. Must resolve in current PR.
- **S2**: Warn in CI. Must resolve within sprint / current PR where applicable.
- **S3**: Advisory. Track for governance review.

All violations produce a structured report:

```
CONSTITUTIONAL_GUARD_VIOLATION:
  section: <WHYCEPOLICY | Determinism | DeterministicIdentifiers | HashDeterminism | ReplayDeterminism>
  command: <command name if applicable>
  policy: <policy name if applicable>
  file: <path>
  rule: <rule id>
  severity: <S0-S3>
  violation: <description>
  expected: <correct state>
  actual: <detected gap>
  remediation: <fix instruction>
```

Policy-binding guard output retains its YAML form:

```yaml
guard: policy-binding
status: PASS | FAIL
policyId: "<resolved policy ID>"
version: "<policy version>"
scope: [<covered scopes>]
ruleCount: <number>
blockRuleCount: <number>
violations:
  - rule: "PB-XX"
    severity: "S0-CRITICAL | S1-HIGH | S2-MEDIUM"
    message: "<description>"
    action: "BLOCK | WARN"
decision: ALLOW | BLOCK
reason: "<human-readable summary>"
```

---

## CI Phase (Policy Binding)

- **pre-commit**: Not required
- **pre-push**: Required for all domain/engine/runtime changes
- **pull-request**: Required
- **merge**: Required
- **deploy**: Required

## Registry Entry

Registered in `guard.registry.json` as `constitutional` with severity `blocking`. Supersedes the source entries for `policy`, `policy-binding`, `determinism`, `deterministic-id`, `hash-determinism`, and `replay-determinism` guards.

## Relationship to Other Guards

This guard does not replace `behavioral.guard.md`, `domain.guard.md`, `engine.guard.md`, or `runtime.guard.md` GE-01 sections — it supplements them by widening the enforcement surface to platform adapters and consolidating the WHYCEPOLICY / determinism / hashing / replay surfaces into one constitutional artifact. Where this guard and an existing guard overlap, the stricter rule wins.

Related audits:
- `claude/audits/replay-determinism.audit.output.md` — the by-design source-of-truth for the sentinel pattern.
- Any audit under `claude/audits/**` that references policy binding, determinism, HSID, hash composition, or replay envelope equality.

## Provenance

Consolidated 2026-04-14 from:

- `policy.guard.md`
- `policy-binding.guard.md` (WBSM v3 | CANONICAL | VERSION 1.1)
- `determinism.guard.md` (including 2026-04-07, 2026-04-10, 2026-04-13 integrated new-rules)
- `deterministic-id.guard.md` (HSID v2.1, including H2–H6 hardening additions)
- `hash-determinism.guard.md`
- `replay-determinism.guard.md` (including 2026-04-07 policy-eventification integration)

## System Invariants & Enforcement Doctrine

Tier-0 constitutional invariants. Cross-layer guarantees not owned by any single guard. WHYCEPOLICY is the control authority; runtime enforces; chain anchors evidence. No invariant below may be bypassed by any lower-tier guard.

### 1. System Execution Invariants

#### INV-001 — Command Outcome Totality

Definition:
Every command MUST produce either ≥1 domain event or an explicit `CommandResult.Failure`. No silent-success paths are permitted.

Enforcement:
The runtime command dispatcher MUST emit a structured `CommandResult` for every command, and that result MUST carry either an event list (≥1 entry) or a typed failure with canonical reason. Empty event lists on `Success` are an enforcement-stage error.

Violation:
A command that returns success without events, or returns void/null, or fails without a typed failure result.

Severity:
S0

References:
- `runtime.guard.md` R7 (persist/publish/anchor)
- `runtime.guard.md` GE-04 (event-first architecture)
- `runtime.guard.md` R-EVENT-AUDIT-COLS-01

---

#### INV-002 — Persist · Anchor · Outbox Atomicity

Definition:
Every successful execution MUST persist its events, anchor the execution to WhyceChain, and enqueue the resulting envelope to the outbox — as one indivisible unit of work.

Enforcement:
The runtime control plane MUST commit (events, chain-anchor row, outbox row) inside a single transaction. Partial commits are forbidden. On crash mid-commit, recovery MUST restore the atomic boundary, never a partial state.

Violation:
Events persisted without chain anchor; chain anchor written without outbox row; outbox row published without persisted event lineage.

Severity:
S0

References:
- `runtime.guard.md` R7
- `runtime.guard.md` R-UOW-01
- WhyceChain anchoring rules

---

#### INV-003 — No Silent Success

Definition:
A command path that returns success without producing observable evidence (event, decision capture, chain anchor) is a constitutional violation.

Enforcement:
Audit gate `RULES_CHECKED ⇒ INV-003` scans dispatcher exit points for any branch that returns `Success` without invoking the persist/anchor/outbox triplet.

Violation:
Code path returning `CommandResult.Success` without writing to the event store, the chain table, and the outbox.

Severity:
S0

References:
- INV-001, INV-002
- `runtime.guard.md` GE-04

---

### 2. Economic Integrity Invariants

#### INV-101 — Double-Entry Enforcement

Definition:
Every value-affecting transaction MUST satisfy the double-entry invariant: the sum of debits equals the sum of credits, per ledger, per transaction boundary.

Enforcement:
The ledger aggregate MUST refuse to commit any transaction whose debit/credit lines do not balance to zero. The check MUST execute before event emission, not after.

Violation:
A persisted ledger transaction whose lines do not balance.

Severity:
S0 — CRITICAL (financial integrity breach)

References:
- `domain.guard.md` ledger invariants
- WHYCEPOLICY economic policy bindings

---

#### INV-102 — Conservation of Value

Definition:
Value MAY NOT be created or destroyed outside policy-authorized flows. Every value movement MUST originate from a typed economic intent that has passed WHYCEPOLICY pre-execution authorization.

Enforcement:
The runtime MUST reject any ledger mutation whose causation chain does not terminate at a policy-authorized economic intent. Mints, burns, write-offs, and adjustments are economic intents and MUST carry policy decisions.

Violation:
A ledger entry without an upstream policy-authorized intent; an adjustment booked outside the authorized adjustment intent.

Severity:
S0 — CRITICAL (financial integrity breach)

References:
- WHYCEPOLICY authority (this guard, Group A–B)
- `domain.guard.md` economic-system rules

---

#### INV-103 — Settlement Irreversibility

Definition:
Once a settlement transaction is committed and chain-anchored, it MUST NOT be mutated, deleted, or re-executed. Reversal is modelled as a compensating transaction, never as state mutation.

Enforcement:
The settlement aggregate MUST refuse update/delete operations on committed transactions. Compensations MUST be authored as new, separately-anchored transactions referencing the original via causation.

Violation:
An attempt to update or delete a committed settlement; a "reversal" implemented as state rewind rather than compensating entry.

Severity:
S0 — CRITICAL (financial integrity breach)

References:
- `domain.guard.md` settlement invariants
- INV-102

---

#### INV-104 — Reconciliation Convergence

Definition:
Internal economic truth and external counterparty truth MUST converge. Reconciliation runs that fail to converge MUST surface as policy-evaluated discrepancies, never as silent data drift.

Enforcement:
Reconciliation processes MUST emit a typed reconciliation event on every run, including a convergence verdict. Non-converging runs MUST trigger a policy-bound discrepancy capture and HALT downstream economic flows until resolved.

Violation:
A reconciliation run that completes silently without verdict; a discrepancy ignored or swept; downstream economic flows continuing on a non-converged ledger.

Severity:
S0 — CRITICAL (financial integrity breach)

References:
- `domain.guard.md` reconciliation invariants (RC1..RC5)
- WHYCEPOLICY reconciliation policy bindings

---

### 3. Identity & Trust Invariants

#### INV-201 — Mandatory Actor Context

Definition:
Every command MUST carry both an `ActorId` and a `WhyceID` context. Commands missing either field MUST be rejected at the runtime entry seam.

Enforcement:
Runtime context propagation MUST extract and validate `ActorId` and `WhyceID` before policy binding. Missing fields produce `NoActorContext` rejection prior to any policy evaluation.

Violation:
A command processed without `ActorId` or `WhyceID`; a context object with null/empty actor or whyceID fields reaching the dispatcher.

Severity:
S0

References:
- `runtime.guard.md` R-CTX-01 (context propagation)
- WHYCEPOLICY enforcement (this guard, Group A)

---

#### INV-202 — No Anonymous Execution

Definition:
Anonymous execution is FORBIDDEN. There is no system actor, no default actor, no fallback identity. Every execution traces to a real, declared identity.

Enforcement:
The actor resolver MUST reject sentinel/empty/anonymous identities. System-level operations (replay, projection rebuild, scheduled jobs) MUST execute under declared service identities, not anonymous contexts.

Violation:
Any command processed under an empty, default, or sentinel actor; any internal job executing without a declared service identity.

Severity:
S0

References:
- INV-201
- WHYCEPOLICY enforcement

---

#### INV-203 — Trust Score Mandatory

Definition:
A `TrustScore` MUST be present on the command context and MUST be policy-evaluated before execution proceeds.

Enforcement:
Pre-execution policy binding (PB-gate) MUST consult `TrustScore` as a policy input. Commands lacking a resolvable `TrustScore` MUST be rejected with a typed identity-evaluation failure.

Violation:
A command executed without TrustScore evaluation; a policy decision rendered without TrustScore as input.

Severity:
S0

References:
- WHYCEPOLICY pre-execution gate (this guard, Group A)
- INV-201

---

#### INV-204 — Privileged Action Traceability

Definition:
All privileged actions MUST be traceable end-to-end to a declared identity, with the identity captured in the chain-anchored decision record.

Enforcement:
The decision capture pipeline MUST persist the resolving `ActorId`, `WhyceID`, and `TrustScore` snapshot on every privileged-action policy decision. Audit must be able to reconstruct the actor lineage from chain anchors alone.

Violation:
A privileged action whose chain anchor lacks identity fields; a decision record where actor lineage cannot be reconstructed.

Severity:
S0

References:
- INV-201, INV-202, INV-203
- WhyceChain anchoring rules

---

### 4. Event & Audit Invariants

#### INV-301 — State Change ⇒ Event

Definition:
Every state change MUST emit at least one event. State mutations without corresponding events are forbidden.

Enforcement:
The aggregate base MUST refuse to commit state changes that do not produce events. Direct state mutation outside the event-emission path is a constitutional violation.

Violation:
An aggregate field changed without a corresponding domain event; persistence layer writes that bypass the event stream.

Severity:
S0

References:
- `runtime.guard.md` GE-04 (event-first)
- `runtime.guard.md` R-EVENT-AUDIT-COLS-01

---

#### INV-302 — Mandatory Event Metadata

Definition:
Every emitted event MUST carry the canonical metadata fields: `EventId`, `CorrelationId`, `CausationId`, `ExecutionHash`, `PolicyDecisionHash`.

Enforcement:
The event envelope constructor MUST validate the presence of all five fields and reject envelopes missing any. Audit gate verifies field presence on every persisted envelope.

Violation:
An emitted or persisted event missing any of the five canonical metadata fields; a null/empty value in any required field.

Severity:
S0

References:
- `runtime.guard.md` R-EVENT-AUDIT-COLS-01
- `runtime.guard.md` R-CHAIN-CORRELATION-01

---

#### INV-303 — Persisted Events Are Chain-Anchored & Replay-Safe

Definition:
Every persisted event MUST be chain-anchored at commit time and MUST be replay-safe (deterministic re-derivation of state from the event stream alone).

Enforcement:
The event store commit path MUST write the chain anchor row inside the same transaction. Replay paths MUST reconstruct state purely from event history without consulting external mutable sources.

Violation:
A persisted event without a corresponding chain-anchor row; an event whose replay produces non-deterministic state.

Severity:
S0

References:
- INV-002 (persist · anchor · outbox)
- `runtime.guard.md` PROJ-REPLAY-SAFE-01
- WhyceChain anchoring rules

---

### 5. Workflow Integrity Invariants

#### INV-401 — Workflow Lifecycle Totality

Definition:
Every workflow MUST traverse the lifecycle: `start → progress → terminate (Completed | Failed)`. Workflows that begin MUST eventually terminate.

Enforcement:
The workflow engine MUST emit a terminal event (`WorkflowCompleted` or `WorkflowFailed`) for every started workflow. Long-running workflows MUST register a deadline and emit a terminal event on deadline expiry.

Violation:
A started workflow without a terminal event; a workflow stuck `InProgress` past its declared deadline without emission.

Severity:
S1

References:
- `runtime.guard.md` R-WF-EVENTIFIED-01
- `runtime.guard.md` R-WORKFLOW-PIPE-01

---

#### INV-402 — No Orphan Workflows

Definition:
Orphan workflows (no parent causation, no terminal event, no resume token) are FORBIDDEN.

Enforcement:
Workflow registration MUST establish parent causation at start. Audit gate scans workflow state for orphans on every run; orphans MUST be reconciled by either resume, compensation, or explicit administrative termination.

Violation:
A workflow row with neither parent causation nor terminal event for >N policy-defined hours.

Severity:
S1

References:
- INV-401
- `runtime.guard.md` R-WF-RESUME-01

---

#### INV-403 — Deterministic Resume

Definition:
Workflow resume MUST be deterministic: it MUST replay from event history alone, with no consultation of mutable external state.

Enforcement:
The resume service MUST reconstruct workflow state by replaying persisted events through pure step handlers. Reads of live mutable state during resume are forbidden.

Violation:
Resume code that reads from live caches, current time, current external API state, or any non-event source to reconstruct workflow state.

Severity:
S0

References:
- `runtime.guard.md` R-WF-RESUME-01
- `runtime.guard.md` REPLAY-A-vs-B-DISTINCTION-01
- INV-303

---

#### INV-404 — Step Output Reproducibility

Definition:
Every workflow step output MUST be reproducible from the event history that preceded it.

Enforcement:
Step handlers MUST be pure functions of (input event, prior workflow state). Step outputs MUST hash identically on replay given identical inputs.

Violation:
A step whose output varies between runs given the same event history; a step that consumes ambient state (clock, RNG, environment).

Severity:
S0

References:
- INV-403
- `runtime.guard.md` GE-01 (deterministic execution)
- `runtime.guard.md` PROJ-REPLAY-SAFE-01

---

### 6. Observability & Operability Invariants

#### INV-501 — Mandatory Telemetry Emission

Definition:
Every command MUST emit, at minimum: a distributed trace, runtime metrics (latency, outcome, decision count), and a structured `CommandResult`.

Enforcement:
The runtime control plane MUST wrap every command execution in a trace span and a metrics scope. Missing trace context or metrics on a completed command is an enforcement violation.

Violation:
A command path that returns without a trace span; a metrics scope not closed; a `CommandResult` returned without structured fields populated.

Severity:
S1

References:
- Phase 1.5 runtime rules (`runtime.guard.md` R-RT-01..10)
- `runtime.guard.md` DET-STOPWATCH-OBSERVABILITY-01

---

#### INV-502 — Canonical Failure Reasons

Definition:
Every failure MUST produce a canonical, typed reason code. Free-text-only failures are FORBIDDEN.

Enforcement:
`CommandResult.Failure` and all typed failure exceptions MUST carry a canonical reason from the enumerated reason registry. Free-text fields MAY accompany the canonical reason but MAY NOT replace it.

Violation:
A failure raised or returned with only a free-text message; a failure whose reason is not in the canonical reason registry.

Severity:
S1

References:
- INV-001
- WHYCEPOLICY decision capture (this guard, Group E)

---

#### INV-503 — Infrastructure Health & Reason Mapping

Definition:
Every infrastructure dependency MUST expose a health check endpoint, and every health-check failure MUST map to a canonical reason consumable by the runtime.

Enforcement:
Each adapter MUST register a health check at composition. The health-check aggregator MUST translate adapter-specific failure modes into canonical reasons before surfacing them to the runtime.

Violation:
An adapter without a registered health check; a health-check failure surfaced as opaque exception text rather than canonical reason.

Severity:
S1

References:
- `infrastructure.guard.md` platform/systems/kafka/config rules
- INV-502

---

### Guard Precedence Doctrine

Order of authority:

1. Constitutional Guard (this file)
2. Runtime Guard
3. Domain Guard
4. Infrastructure Guard

If a conflict occurs:
- Higher layer overrides lower layer
- Constitutional rules are final and non-overridable

### 7. Invariant–Policy Binding Doctrine

Every invariant MUST be enforceable via WHYCEPOLICY. No invariant may exist without a corresponding policy evaluation path. Every invariant check MUST produce a policy decision record.

#### INV-POL-001 — Invariant Requires Policy Binding

Definition:
Every invariant declared in this guard MUST map to at least one WHYCEPOLICY rule or decision path capable of evaluating conformance at runtime.

Enforcement:
The policy registry MUST expose a binding entry for each invariant ID. Unbound invariants fail the pre-execution policy-binding stage and halt the pipeline.

Violation:
An invariant referenced in guards, audits, or runtime checks that has no corresponding WHYCEPOLICY rule or decision path.

Severity:
S0

References:
- `constitutional.guard.md` WHYCEPOLICY Authority — Policy Declaration and Binding
- GE-02 (WHYCEPOLICY ENFORCEMENT)
- CLAUDE.md $8 (Policy)

---

#### INV-POL-002 — Policy Decision on Invariant Evaluation

Definition:
Every invariant evaluation MUST produce a `PolicyDecision` carrying an explicit allow/deny outcome. Implicit or inferred conformance is forbidden.

Enforcement:
Runtime invariant checks MUST call through the policy evaluator and persist the returned `PolicyDecision` to the evaluation audit trail.

Violation:
An invariant evaluation that completes without emitting a `PolicyDecision`, or returns a boolean/exception in place of the decision record.

Severity:
S0

References:
- `constitutional.guard.md` Policy Evaluation Audit Trail
- INV-POL-001

---

#### INV-POL-003 — Invariant Violations Are Policy Violations

Definition:
Any invariant violation MUST be treated as a policy denial event and handled through the same denial pathway as native WHYCEPOLICY deny outcomes.

Enforcement:
The runtime MUST route invariant-failure results into the policy denial handler — never into ad-hoc exception branches — producing a denial envelope with canonical reason.

Violation:
An invariant failure surfaced as a generic exception, log-only message, or bypassed denial handler.

Severity:
S0

References:
- INV-POL-002
- INV-502 (Canonical Failure Reasons)

---

#### INV-POL-004 — Policy Evidence Anchoring

Definition:
All invariant-related policy decisions MUST be chain-anchored to WhyceChain alongside the execution they govern.

Enforcement:
The policy decision record for every invariant evaluation MUST be included in the execution's chain-anchor payload and committed atomically with the execution UoW.

Violation:
A policy decision for an invariant that is not present in the chain-anchor row for its execution.

Severity:
S0

References:
- `constitutional.guard.md` Policy Decision Hashing and Chain Anchoring
- GE-03 (WHYCECHAIN ANCHORING)
- INV-002 (Persist · Anchor · Outbox Atomicity)

---

### 8. Invariant Violation Handling Doctrine

Defines system behavior when invariants fail. Handling is severity-driven and MUST be uniform across all guards.

#### INV-HAND-001 — S0 Immediate Halt

Definition:
Any S0 invariant violation MUST immediately halt the execution pipeline. No retry, no fallback, no degraded-mode continuation is permitted.

Enforcement:
The runtime control plane MUST terminate the current unit of work on S0 detection, emit the structured failure report, and refuse further stage progression.

Violation:
An S0 violation followed by retry, compensating action, or continued stage execution.

Severity:
S0

References:
- CLAUDE.md $12 (Failure)
- `constitutional.guard.md` Severity Matrix
- INV-001 (Command Outcome Totality)

---

#### INV-HAND-002 — S1 Controlled Failure

Definition:
S1 invariant violations MAY permit controlled retry or fallback, but only when the retry/fallback path is itself policy-evaluated and approved.

Enforcement:
Any retry or fallback path for an S1 violation MUST request a `PolicyDecision` before execution and record the decision in the audit trail.

Violation:
An S1 retry or fallback executed without a preceding policy evaluation or without decision recording.

Severity:
S1

References:
- INV-POL-002
- `constitutional.guard.md` Severity Matrix

---

#### INV-HAND-003 — S2 Degraded Mode

Definition:
S2 invariant violations MAY allow continued execution under degraded mode, provided warning-level telemetry is emitted for every occurrence.

Enforcement:
Degraded-mode continuation MUST emit a canonical warning telemetry event identifying the invariant ID, the degraded capability, and the scope of continuation.

Violation:
Degraded-mode continuation without warning-level telemetry, or silent downgrade of capability.

Severity:
S2

References:
- INV-501 (Mandatory Telemetry Emission)
- `constitutional.guard.md` Severity Matrix

---

#### INV-HAND-004 — Mandatory Incident Emission

Definition:
Every invariant violation, regardless of severity, MUST emit an incident event carrying the invariant ID, severity, canonical reason, and execution context.

Enforcement:
The runtime incident emitter MUST be invoked on every invariant-failure branch; the emitted event MUST conform to the canonical incident schema.

Violation:
An invariant violation that passes without a corresponding incident event, or an incident event missing required fields.

Severity:
S1

References:
- INV-301 (State Change ⇒ Event)
- INV-302 (Mandatory Event Metadata)

---

#### INV-HAND-005 — Chain-Anchored Violation Record

Definition:
All invariant violations MUST be recorded and anchored in WhyceChain as part of the execution's evidence lineage.

Enforcement:
The violation record (invariant ID, severity, policy decision, incident event reference) MUST be committed to the chain-anchor row for the execution that produced it.

Violation:
A detected invariant violation with no corresponding WhyceChain anchor entry.

Severity:
S0

References:
- INV-POL-004
- INV-303 (Persisted Events Are Chain-Anchored & Replay-Safe)
- GE-03 (WHYCECHAIN ANCHORING)

---

#### INV-IDEMPOTENT-LIFECYCLE-INIT-01 — Layered-Defense Idempotency For Lifecycle-Init Aggregates (S1)

Definition:
Aggregates whose lifecycle-init action is a `public static` factory method (constructing a fresh aggregate instance and raising the seed event) MUST be guarded by a three-layer idempotency composition. The single-layer `IdempotencyMiddleware` cache is insufficient across hosts; the storage UNIQUE constraint alone surfaces untyped `Npgsql.PostgresException` → HTTP 500 violating INV-502.

Canonical three-layer model:

| Layer | Surface | Lifetime | Catches | Returns |
|---|---|---|---|---|
| 1. `IdempotencyMiddleware` | runtime middleware pipeline | per-host process (in-memory cache) | duplicate dispatch from same host before cache eviction | `CommandResult.Failure("Duplicate command detected.")` → HTTP 400 |
| 2. Engine handler load-then-guard | engine handler invoking a static-factory aggregate | persistent (event-store query per dispatch) | duplicate dispatch from a different host, after a restart, after cache eviction | typed `<Domain>Errors.Already<X>Recorded` `DomainException` → HTTP 400 via `DomainExceptionHandler` |
| 3. `(aggregate_id, version)` UNIQUE | Postgres event-store INSERT | persistent + cross-host (storage constraint) | last-resort race where two handlers both passed Layer 2 simultaneously | `ConcurrencyConflictException` → HTTP 500 (not canonical-reason-typed) |

Enforcement:
Every T2E handler that targets a `public static` factory aggregate MUST (1) inject `IEventStore` (or equivalent persistent prior-state probe); (2) call `IEventStore.LoadEventsAsync(aggregateId)` BEFORE invoking the static factory; (3) if `prior.Count > 0`, throw a typed `<Domain>Errors.Already<X>Recorded` / `Already<X>Created` `DomainException` — the error message MUST contain the substring matching the literal noun in the rule name (e.g. "already recorded"); (4) only invoke the factory and emit events when `prior.Count == 0`.

Static check: every engine handler that dispatches to a static-factory aggregate MUST inject `IEventStore` and contain the load-then-throw pattern. Any handler that calls a static factory without first probing the event store is a violation. Cross-host validation test is mandatory (fresh handler instance against an `IEventStore` already holding the seed event, asserting the typed exception).

Violation:
A lifecycle-init handler that relies solely on `IdempotencyMiddleware` (Layer 1) — the cross-host/post-restart scenario bypasses it; or a handler that relies solely on the UNIQUE constraint (Layer 3) — its failure surface is untyped and breaks INV-502.

Severity:
S1 — replay correctness and canonical-reason discipline. The shadowing of Layer 2 by Layer 1 in same-host tests is by design (defense-in-depth, not redundancy); removing Layer 2 because Layer 1 fires first in some paths would erode the cross-host guarantee.

References:
- INV-502 (Canonical Failure Reasons)
- `domain.guard.md` DOM-LIFECYCLE-INIT-IDEMPOTENT-01 (instance-method shape)
- INV-001 (Command Outcome Totality), INV-303 (Replay-Safe)
**Source:** new-rules 2026-04-16 (revenue-domain validation D5).

---

### 9. Audit Coverage Doctrine

Every guard rule MUST be verifiable by a canonical audit. Audit coverage is a first-class constitutional property, not an optional sweep.

#### AUD-COV-001 — Every Rule Must Be Audited

Definition:
Every rule declared in any canonical guard MUST appear in at least one canonical audit under `claude/audits/`.

Enforcement:
The audit registry MUST enumerate rule IDs covered. Uncovered rules are flagged by the coverage sweep and fail the post-execution audit stage.

Violation:
A guard rule ID absent from every canonical audit's coverage manifest.

Severity:
S1

References:
- CLAUDE.md $1b (Post-Execution Audit Sweep)

---

#### AUD-COV-002 — Missing Audit Is Violation

Definition:
A guard rule without audit coverage constitutes a governance violation in its own right and MUST be captured under `/claude/new-rules/` per CLAUDE.md §1c.

Enforcement:
The coverage sweep MUST raise a drift capture for every uncovered rule and halt the sweep until the capture is recorded.

Violation:
An uncovered rule detected without a corresponding new-rules capture entry.

Severity:
S1

References:
- CLAUDE.md $1c (New Rules Capture)
- AUD-COV-001

---

#### AUD-COV-003 — Audit Must Reference Rule ID

Definition:
All audits MUST validate behavior by referencing canonical rule IDs. Free-text validation that does not anchor to a declared rule ID is forbidden.

Enforcement:
The audit loader MUST reject any audit whose assertions do not bind to at least one declared rule ID in the canonical guard set.

Violation:
An audit that asserts behavior without citing a rule ID, or cites an ID not present in canonical guards.

Severity:
S2

References:
- AUD-COV-001
- `constitutional.guard.md` Provenance

---

#### AUD-COV-004 — No Duplicate Audit Ownership

Definition:
A rule MUST NOT be validated by multiple audits unless it is explicitly declared shared (e.g., GE-series global-enforcement rules).

Enforcement:
The coverage sweep MUST detect duplicate ownership and fail unless the rule is present on the explicit shared-ownership allowlist.

Violation:
Two or more audits claiming exclusive ownership of the same non-shared rule ID.

Severity:
S2

References:
- GE-01 through GE-05
- AUD-COV-001

---

#### AUD-COV-005 — Audit Reflects Guard Changes

Definition:
Any modification to a canonical guard MUST trigger a corresponding audit update within the same change set.

Enforcement:
Guard edits MUST be accompanied by audit updates covering the modified rule IDs; unpaired guard changes fail the coverage sweep.

Violation:
A guard modification merged without the matching audit update.

Severity:
S1

References:
- CLAUDE.md $1b
- GUARD-VERS-001

---

### 10. Guard Evolution & Versioning Doctrine

Guards are constitutional artifacts. Their evolution MUST be traceable, non-silent, and backward-safe.

#### GUARD-VERS-001 — Guard Changes Require New-Rule Capture

Definition:
All guard changes MUST be recorded via the new-rules capture pathway defined in CLAUDE.md §1c before promotion into canonical guards.

Enforcement:
The guard loader MUST cross-check recent guard modifications against `/claude/new-rules/` entries and fail on uncaptured changes.

Violation:
A guard edit landed in `claude/guards/` without a preceding or accompanying capture under `/claude/new-rules/`.

Severity:
S1

References:
- CLAUDE.md $1c (New Rules Capture)

---

#### GUARD-VERS-002 — No Silent Guard Mutation

Definition:
Guard rules MUST NOT be modified without a traceable change record linking edit, rationale, and authorising policy decision.

Enforcement:
Guard modifications MUST include a provenance entry (author, timestamp, rule IDs touched) and a policy decision reference in the change record.

Violation:
A guard-file diff with no corresponding provenance or policy decision record.

Severity:
S1

References:
- GUARD-VERS-001
- `constitutional.guard.md` Provenance

---

#### GUARD-VERS-003 — Backward Compatibility Enforcement

Definition:
Guard changes MUST NOT break existing audits. Any breaking change MUST be accompanied by simultaneous audit updates in the same change set.

Enforcement:
The change-set validator MUST run the audit coverage sweep against the proposed guard state and reject any diff that leaves audits in a broken state.

Violation:
A guard change merged while one or more audits relying on it are unresolved or failing.

Severity:
S1

References:
- AUD-COV-005
- CLAUDE.md $1b

---

#### GUARD-VERS-004 — Versioned Rule Identity

Definition:
Each rule ID MUST remain stable across guard versions. Rule IDs are immutable identifiers; renaming an active rule is forbidden.

Enforcement:
The guard loader MUST fail if a rule ID present in prior versions disappears without a matching deprecation marker per GUARD-VERS-005.

Violation:
A rule ID silently renamed, removed, or reassigned between versions.

Severity:
S1

References:
- GUARD-VERS-005
- `constitutional.guard.md` Registry Entry

---

#### GUARD-VERS-005 — Deprecation Protocol

Definition:
Deprecated rules MUST be marked explicitly as `REVOKED` or `SUPERSEDED`, including the superseding rule ID where applicable. Silent removal is forbidden.

Enforcement:
Deprecation markers MUST appear inline in the guard file and in the registry entry; the loader MUST treat unmarked removal as drift.

Violation:
A rule removed from a guard without a `REVOKED` or `SUPERSEDED` marker and registry update.

Severity:
S1

References:
- GUARD-VERS-004
- CLAUDE.md $1c

---

### 11. Cross-System Invariants

Light-touch invariants covering state, identity, and ordering across system boundaries. These complement — not replace — the system-specific rules in `infrastructure.guard.md` and `domain.guard.md`.

#### INV-XSYS-001 — Cross-Boundary Consistency

Definition:
State shared across systems MUST remain consistent. Divergent views of the same canonical state across system boundaries are forbidden.

Enforcement:
Cross-system state transfers MUST flow through the canonical event/outbox path and reconcile to a single authoritative projection. Ad-hoc cross-system writes are forbidden.

Violation:
Two systems holding conflicting values for the same canonical state key without a reconciliation path.

Severity:
S1

References:
- INV-104 (Reconciliation Convergence)
- GE-04 (Event-First Architecture)
- GE-05 (CQRS Enforcement)

---

#### INV-XSYS-002 — Distributed Identity Integrity

Definition:
Identity context (actor, tenant, trust score, policy scope) MUST remain intact across system boundaries. No boundary traversal may drop, mutate, or synthesise identity fields.

Enforcement:
Cross-system envelopes MUST carry the canonical identity block; receivers MUST reject envelopes whose identity block is absent, truncated, or re-signed by a non-authoritative source.

Violation:
An envelope crossing a system boundary with missing, altered, or fabricated identity context.

Severity:
S0

References:
- INV-201 (Mandatory Actor Context)
- INV-202 (No Anonymous Execution)
- INV-203 (Trust Score Mandatory)

---

#### INV-XSYS-003 — Event Ordering Integrity

Definition:
Cross-system event flows MUST preserve causation order. Causally related events MUST be observed in the same relative order on every consuming system.

Enforcement:
Cross-system transport MUST carry causation metadata (causation ID, sequence); consumers MUST reject or quarantine envelopes that violate causation ordering.

Violation:
A consumer applying a caused event before its cause, or reordering causally linked events during transport.

Severity:
S1

References:
- INV-301 (State Change ⇒ Event)
- INV-302 (Mandatory Event Metadata)
- `infrastructure.guard.md` kafka ordering rules
