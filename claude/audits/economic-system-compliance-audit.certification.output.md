---
type: e2e-certification
classification: economic-system
context: compliance
domain: audit
date: 2026-04-16
prompt: claude/project-prompts/20260416-111528-economic-system-compliance-audit-e2e-certification.md
status: FAIL
---

# E2E Certification Output — `economic-system / compliance / audit`

## Input Parameter Notice

The submitted prompt declared `DOMAIN GROUP: compliance` under context `compliance`, implying a 4-level path `src/domain/economic-system/compliance/compliance/audit/`. The canonical CLAUDE.md model is **3-level (`classification/context/domain`)**, and the actual code lives at `src/domain/economic-system/compliance/audit/`. Certification was performed against the actual canonical path.

---

## 1. Overall Status

**FAIL** (Mandatory Failure Rule triggered: Kafka publishing surface is incomplete — topic missing from broker bootstrap.)

---

## 2. Per-Domain Status

| Domain | Status | Notes |
|---|---|---|
| `audit` | **FAIL** | Domain layer is S4-clean. Pipeline, schema registration, and projection are wired. Kafka topic provisioning + migration + tests are absent. |

---

## 3. Infrastructure Status

| System | Status | Notes |
|---|---|---|
| Postgres (event store) | **CONDITIONAL PASS** | Event-store schema + audit-column migrations exist (`003_event_store_audit_columns.sql`, `004_event_store_audit_not_null.sql`). Stream/version persistence is generic. |
| Postgres (projection table) | **FAIL** | No DDL/migration for `projection_economic_compliance_audit.audit_record_read_model`. Schema does not exist in `infrastructure/data/postgres/projections/`. Projection writes will fail at runtime unless created out-of-band. |
| Kafka | **FAIL** | Topic `whyce.economic.compliance.audit.events` is **not** declared in `infrastructure/event-fabric/kafka/create-topics.sh` (verified by direct grep — zero matches for `audit` / `compliance`). Violates **R-K-17 (K-TOPIC-02)** and **R-K-20 (K-TOPIC-COVERAGE-01, S0)**. Outbox publish will deadletter or fail. |
| Redis | **N/A → PASS** | Not used by audit domain; rule R-K-* / projection-redis exclusion respected. |
| OPA / WHYCEPOLICY | **CONDITIONAL PASS** | Three policy IDs declared (`whyce.economic.compliance.audit.{create,finalize,read}`). Two command bindings registered via `CompliancePolicyModule`. **Read action policy id is unbound** and no external policy file under `claude/registry/` or `infrastructure/policies/` was located — relies on the central policy engine being preloaded, which is not evidenced in-repo. |

---

## 4. Section-by-Section Findings

### §2 Scope & Structure — **PASS (with input mismatch)**
- Canonical 3-level path correct: `src/domain/economic-system/compliance/audit/`.
- All 7 mandatory subfolders present (`aggregate`, `event`, `value-object`, `error`, `specification`, `service`, `README.md`; `entity/` justified absent).
- Lowercase naming respected.
- `-system` classification suffix on domain root respected (per domain.guard.md `CLASS-SFX-R1`).
- Satisfies **D67** (compliance domain required), `domain.guard.md` lines 1055–1056.

### §3 Domain Model (E1 / S4) — **PASS**
- `AuditRecordAggregate` (sealed, lifecycle Draft→Finalized, terminal-state guard at line 37, invariant block `EnsureInvariants()` 65–71): satisfies **E2/E4/E6/D69/D71/D74**.
- 8 value objects, all `readonly record struct` with constructor validation (empty-Guid / whitespace rejection).
- Determinism: zero hits for `Guid.NewGuid`, `DateTime.UtcNow`, `Random`, `HttpClient`, `DbContext`, EF, Dapper, `IRepository` inside the domain folder. Satisfies **GE-01 / DET-***.
- Events `AuditRecordCreatedEvent` / `AuditRecordFinalizedEvent` follow `{Aggregate}{Action}Event` past-tense naming; payloads carry full source-reference set.
- `AuditErrors` exposes 5 typed string constants (no generic exceptions for business logic).
- `AuditRecordSpecification` is a pure stateless rule pair (`CanFinalize`, `IsFinalized`).

### §4 Command Layer (E2) — **PASS**
- `CreateAuditRecordCommand` and `FinalizeAuditRecordCommand` defined in `src/shared/contracts/economic/compliance/audit/AuditRecordCommands.cs` as immutable records.
- Field shape mirrors aggregate factory parameters; no business logic in commands; no infrastructure references.
- Note: commands carry `RecordedAt` / `FinalizedAt` as `DateTimeOffset` from caller — provided that values originate from the API layer (which itself MUST source from `IClock`), this is acceptable; the responsibility is shifted into the controller and the controller MUST honor the determinism rule.

### §5 Query Layer (E3) — **CONDITIONAL PASS**
- `AuditRecordReadModel` defined and used by API/projection.
- **Gap:** No `Audit*Query*` contract; the `GetAudit` endpoint reaches the projection table via raw SQL inside the controller. CQRS separation is intact (read model never feeds back to write side), but the absence of a query abstraction is a non-critical gap (reduces testability and hides a controller-layer infra dependency). Track per **GE-05**.

### §6 Engine Handlers (E4 — T2E) — **PASS**
- `CreateAuditRecordHandler` and `FinalizeAuditRecordHandler` under `src/engines/T2E/economic/compliance/audit/`.
- Both implement `IEngine`, receive command via `RuntimeControlPlane` dispatcher (R-RT-01), construct/load aggregate, emit events through the runtime context.
- No direct DB access; rely on `context.LoadAggregateAsync` / `context.EmitEvents`.
- Idempotency on `Finalize` enforced inside the aggregate (Already-Finalized error). Runtime-level `IdempotencyMiddleware` (locked pipeline position 7) covers retried command IDs.

### §7 Policy Integration (E5) — **CONDITIONAL PASS**
- Policy IDs declared canonically: `whyce.economic.compliance.audit.{create,finalize,read}`.
- Two `CommandPolicyBinding` entries registered in `CompliancePolicyModule`.
- **Gap A:** `read` policy id is declared but no binding was located.
- **Gap B:** No policy file evidenced in source. Per **constitutional.guard.md PB-08** (POLICY SOURCE VALIDATION, S0), policies MUST originate from an external authoritative source (e.g., `registry/policies.json` or OPA). Audit could not verify the existence of such a file for the audit-domain bindings. Until verified, this is a S0 risk against the certification.
- Decision/chain anchoring (`PB-09`, `PB-10`, `POL-AUDIT-14/15/16`) is the runtime control-plane's responsibility and not domain-specific; the audit domain inherits the runtime guarantees provided the bindings resolve.

### §8 Event Fabric (E6 — Kafka) — **FAIL (S0)**
- **R-K-20 (K-TOPIC-COVERAGE-01, S0)** violated. Verified by direct grep of `infrastructure/event-fabric/kafka/create-topics.sh`: **zero matches for `audit` or `compliance`** in 264 lines of the script. The two declared event types (`AuditRecordCreatedEvent`, `AuditRecordFinalizedEvent`) are registered in `EconomicSchemaModule.RegisterComplianceAudit()` and the consumer worker is wired to topic `whyce.economic.compliance.audit.events`, but the broker has no provisioning step for that topic.
- **R-K-17 (K-TOPIC-02)** violated as a corollary.
- **R-K-22 (K-TOPIC-DOC-CONSISTENCY-01, S3)** violated: the domain README documents the topic name; documentation references a topic the broker bootstrap does not create.
- Topic naming format `whyce.economic.compliance.audit.events` (5 segments) — this is the project-wide pattern used for every other event topic in `create-topics.sh`. **R-K-18 (K-TOPIC-CANONICAL-01)** specifies "4–5 segments" — within tolerance.
- Outbox flow itself is generic and event-driven (no audit-specific publish code observed); when the topic exists, **R-K-13** (runtime outbox only) is honored.

### §9 Postgres / Event Store / Projection — **PARTIAL FAIL**
- **Event Store (write side):** generic schema applies; audit-envelope columns covered by migrations 003/004. **PASS**.
- **Outbox:** generic outbox table is shared; audit events route through it once the topic exists.
- **Projection store DDL:** `EconomicProjectionModule` registers `PostgresProjectionStore<AuditRecordReadModel>` and a `GenericKafkaProjectionConsumerWorker` for the schema `projection_economic_compliance_audit` / table `audit_record_read_model`. **No DDL or migration file declaring this schema/table exists** (verified: `infrastructure/data/postgres/**/*audit*` returned only event-store audit-column files; `**/*compliance*` returned zero). Projection upserts will fault at first dispatch unless a hidden runtime-bootstrap step creates the table — none documented.
- **Consequence:** Mandatory criterion "projection update" cannot succeed end-to-end → **FAIL**.

### §10 Redis — **PASS (N/A)**
- Audit domain uses no Redis; no caching or distributed-lock dependency. Compliant by exclusion.

### §11 Workflow (E9) — **PASS (justified absence)**
- README documents intentional omission: two-state bounded domain, no orchestration justified. Aligns with the runtime-guard policy that workflows are reserved for long-running / multi-step / cross-domain flows.

### §12 API Layer (E8) — **CONDITIONAL PASS**
- `AuditController` under `src/platform/api/controllers/economic/compliance/audit/AuditController.cs`.
- Routes: `POST /api/compliance/audit/create`, `POST /api/compliance/audit/finalize`, `GET /api/compliance/audit/{id:guid}`.
- Dispatch via `ISystemIntentDispatcher` (satisfies **R-PLAT-12 / PLAT-DISPATCH-ONLY-01**).
- `[Authorize]` decoration; controllers carry no domain logic.
- **Gap:** Route prefix is `api/compliance/...` rather than the canonical `/api/{classification}/{context}/{domain}/...` (`/api/economic/compliance/audit/...`). The `economic` classification segment is missing from the URL space. Per the prompt's §12 routing convention, this is a **non-critical deviation** worth tracking for governance consistency.
- **Gap:** GET endpoint runs raw SQL against the projection table inline. Move to a query handler when query layer is added (§5).

### §13 End-to-End — **FAIL**
- **Cannot be executed end-to-end at present.** Two infrastructure preconditions are missing (Kafka topic, projection DDL). Even if the API → command → handler → aggregate → event-store → chain segment passes, the **outbox→Kafka→projection** segment cannot complete:
  - Outbox publisher would attempt to produce to an undeclared topic; per **R-K-21 (K-OUTBOX-ISOLATION-01)** the row would be marked `failed` rather than crash the host, but the event would never reach the consumer.
  - Even with the topic, the projection consumer would attempt to upsert into a schema that does not exist.
- No automated E2E tests exist for the domain (verified: `tests/**/*audit*` and `tests/**/*Audit*` returned zero). The README documents the expected E2E path but it is unexercised.

### §14 Observability (E10) — **CONDITIONAL PASS**
- Generic runtime telemetry covers tracing/metrics/correlation. No audit-domain-specific metric or trace span. README lists desired metrics (`audit.record.created.count`, `audit.record.finalized.count`, `audit.record.finalize_latency_seconds`) but they are not implemented.
- Acceptable as a non-critical gap because **INV-501** is satisfied at runtime level for every command.

### §15 Security & Enforcement (E11) — **CONDITIONAL PASS**
- `[Authorize]` enforced on the controller; runtime layer applies actor + WhyceID + trust score under **INV-201/202/203**. Audit domain inherits these.
- No explicit restricted-operation gating beyond the policy bindings. Per **INV-204** (privileged-action traceability), once chain anchoring is functioning end-to-end, audit decisions will surface in the chain — currently blocked by the §8/§9/§13 failures.

---

## 5. Critical Failures (must fix before progression)

| # | Finding | Rule | Severity |
|---|---|---|---|
| C1 | Kafka topic `whyce.economic.compliance.audit.events` not declared in `infrastructure/event-fabric/kafka/create-topics.sh` | R-K-20 / K-TOPIC-COVERAGE-01 | **S0** |
| C2 | Projection schema `projection_economic_compliance_audit` and table `audit_record_read_model` have no DDL/migration in `infrastructure/data/postgres/**` | runtime.guard.md PROJ-* | **S0** (mandatory failure rule: "projection update") |
| C3 | No external policy file evidenced for `whyce.economic.compliance.audit.*` IDs (PB-08 source validation) | constitutional.guard.md PB-08 | **S0** if no external source exists; **PASS** if an external OPA/registry file is present but not in-repo. **REQUIRES VERIFICATION.** |
| C4 | `read` policy id declared but no binding registered | constitutional.guard.md POL-01 | **S0** |
| C5 | No automated tests covering the audit domain (unit, integration, or E2E) | runtime.guard.md test/E2E subsystem | **S1** |

---

## 6. Non-Critical Gaps (trackable)

| # | Finding | Severity |
|---|---|---|
| N1 | Input parameter `DOMAIN GROUP: compliance` does not match canonical 3-level nesting. Certification prompt template should be revised. | S3 |
| N2 | API route `/api/compliance/...` omits the classification segment vs prompt §12 canonical pattern `/api/{classification}/{context}/{domain}/...`. | S2 |
| N3 | No `Audit*Query` contract; controller GET runs raw SQL against the projection table. | S2 |
| N4 | README documents desired metrics that are not implemented; no audit-domain-specific telemetry. | S3 |
| N5 | R-K-22 docs/topic consistency: README references a topic the broker bootstrap script does not create. | S3 |
| N6 | Commands carry caller-supplied `RecordedAt` / `FinalizedAt`. Determinism is preserved only if the controller sources from `IClock`; verify upstream. | S2 |

---

## 7. Evidence Summary

| Layer | Evidence |
|---|---|
| Domain | `src/domain/economic-system/compliance/audit/aggregate/AuditRecordAggregate.cs:1-72`, value-objects (8 files), event/{2 files}, error/AuditErrors.cs, specification/, service/, README.md. Direct grep: zero non-deterministic primitives in domain folder. |
| Commands | `src/shared/contracts/economic/compliance/audit/AuditRecordCommands.cs` |
| Engine handlers | `src/engines/T2E/economic/compliance/audit/{CreateAuditRecordHandler,FinalizeAuditRecordHandler}.cs` |
| Read model | `src/shared/contracts/economic/compliance/audit/AuditRecordReadModel.cs` |
| Projection | `src/projections/economic/compliance/audit/AuditRecordProjectionHandler.cs:11-58`, `reducer/AuditRecordProjectionReducer.cs:6-31`; registration in `EconomicProjectionModule.cs:143-145, 257-258, 426-430`. |
| Schema registration | `src/runtime/event-fabric/domain-schemas/EconomicSchemaModule.cs:995-1027` (`RegisterComplianceAudit`). |
| Policy bindings | `src/shared/contracts/economic/compliance/audit/AuditRecordPolicyIds.cs`, `src/platform/host/composition/economic/compliance/CompliancePolicyModule.cs:9-14`. |
| API | `src/platform/api/controllers/economic/compliance/audit/AuditController.cs:17-124`. |
| Composition | `src/platform/host/composition/economic/compliance/audit/application/ComplianceAuditApplicationModule.cs:10-21`. |
| Event-store DB | `infrastructure/data/postgres/event-store/migrations/003_event_store_audit_columns.sql`, `004_event_store_audit_not_null.sql`. |
| Kafka topic provisioning | `infrastructure/event-fabric/kafka/create-topics.sh` — **no `audit` / `compliance` matches** (verified). |
| Projection DDL | **NONE FOUND** for `projection_economic_compliance_audit.audit_record_read_model`. |
| Tests | **NONE FOUND** under `tests/**/*audit*` or `tests/**/*Audit*`. |

---

## 8. Certification Decision

**BLOCKED — NOT APPROVED FOR PHASE PROGRESSION.**

Rationale: The Mandatory Failure Rule of the certification prompt is triggered by the missing Kafka topic provisioning (kafka publishing) and the missing projection DDL (projection update). The domain code itself is clean and S4-compliant; the failure is at the infrastructure-wiring boundary, not in the domain.

### Required to Unblock (in order)

1. **Add the topic to `create-topics.sh`** — declarations for `whyce.economic.compliance.audit.events` (and a corresponding `.deadletter` per R-K-08). Re-run the bootstrap topic-coverage check.
2. **Add the projection migration** — schema `projection_economic_compliance_audit` and table `audit_record_read_model` (state column matching `AuditRecordReadModel`) under `infrastructure/data/postgres/projections/economic/compliance/audit/migrations/001_audit_record_projection.sql`.
3. **Verify policy source (PB-08)** — confirm an external policy file or OPA bundle exists for the three `whyce.economic.compliance.audit.*` IDs, or add one.
4. **Bind the `read` policy id** in `CompliancePolicyModule` (or evidence that read-side policy is enforced via a query-side middleware).
5. **Add E2E test coverage** exercising create→finalize→read with assertions against event store, Kafka topic, and projection table.
6. **(Recommended)** align API route to `/api/economic/compliance/audit/...` to match prompt §12 canonical pattern.

Once items 1–4 are merged with verifiable evidence (CI-green topic-coverage script, applied migration, policy-source validation, bound read policy), this certification can be re-run. Items 5–6 may be tracked as conditional follow-ups.

---

## 9. Drift Capture Note ($1c)

No new guard rules required — all findings map to existing canonical rules:
- C1 → R-K-20 / K-TOPIC-COVERAGE-01 (existing, S0)
- C2 → projection runtime guard P1–P12 (existing) and infrastructure-readiness expectation
- C3 → constitutional.guard.md PB-08 (existing, S0)
- C4 → constitutional.guard.md POL-01 (existing, S0)
- C5 → runtime.guard.md test/E2E subsystem (existing)

No `/claude/new-rules/` capture necessary.
