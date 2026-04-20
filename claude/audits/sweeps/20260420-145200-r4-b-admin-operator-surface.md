# Post-Execution Audit Sweep — R4.B Admin / Operator Control Surface

Date: 2026-04-20
Prompt: `claude/project-prompts/20260420-145200-runtime-r4-b-admin-operator-surface.md`
Scope: runtime admin surface + operator audit trail + admin policy gate + DLQ re-drive
Stage: $1b (post-execution audit sweep) after successful $1 execution

---

## 1. Guard coverage check

Rules promoted to `claude/guards/runtime.guard.md` → §R4.B Admin / Operator Control Surface:

- [x] R-ADMIN-SCOPE-01 — admin authorization policy gate (pinned by `AdminSurfaceArchitectureTests.Every_admin_controller_inherits_admin_controller_base_or_declares_admin_policy`)
- [x] R-ADMIN-ROUTE-PREFIX-01 — canonical `api/admin` prefix (pinned by `AdminSurfaceArchitectureTests.Every_admin_controller_routes_under_api_admin_prefix`)
- [x] R-ADMIN-NO-AGGREGATE-MUTATION-01 — no domain aggregate import from admin (pinned by `AdminSurfaceArchitectureTests.Admin_controllers_do_not_import_domain_aggregate_namespaces` + `Whycespace.Api.csproj` has NO `Whycespace.Domain` project reference)
- [x] R-ADMIN-AUDIT-EMISSION-01 — every mutating action emits evidence (pinned by `OperatorActionAuditRecorderTests` + `DeadLetterRedriveServiceTests`)
- [x] R-ADMIN-REDRIVE-ELIGIBILITY-01 — three-gate eligibility + publisher-failure safety (pinned by `DeadLetterRedriveServiceTests` all five branches)
- [x] R-ADMIN-RECONCILE-PRECONDITION-01 — reconcile only when ReconciliationRequired (delegated to existing R-OUT-EFF-RECONCILE-PRECONDITION-01; controller maps `InvalidOperationException` → 400 refused)
- [x] R-ADMIN-OP-IDENTITY-01 — operator identity sourced from `ICallerIdentityAccessor.GetActorId()` — never hardcoded, never body-supplied
- [x] R-ADMIN-COMPOSITION-ORDER-01 — admin module pinned at Order = 6 (pinned by `AdminSurfaceArchitectureTests.Admin_composition_module_is_registered_in_registry_at_locked_order`)

`claude/guards/infrastructure.guard.md` → G-COMPLOAD-03 extended to include `Admin(6)` in the locked execution sequence.

---

## 2. Runtime-guard sweep on modified files

### Determinism (§9 / R-ID-GEN-01 / DET-SEED-DERIVATION-01)
- No `Guid.NewGuid()`, `DateTime.UtcNow`, `DateTime.Now`, `DateTime.Ticks`, `new Random()`, or `Stopwatch.GetTimestamp()` in any admin production code under:
  - `src/shared/contracts/runtime/admin/`
  - `src/runtime/control-plane/admin/`
  - `src/platform/host/adapters/admin/`
  - `src/platform/host/authorization/`
  - `src/platform/host/composition/runtime/admin/`
  - `src/platform/api/controllers/platform/admin/`
- `OperatorActionAuditRecorder` uses injected `IClock` + `IIdGenerator`; seed derivation follows canonical `{CorrelationId}:{ActionType}:{TargetId}` pattern (DET-SEED-DERIVATION-01 compliant — no clock entropy in seed).
- `DeadLetterRedriveService` uses injected `IClock` for `reprocessedAt` timestamp only.
- `AdminScope.cs` / `OperatorActionEvent.cs` / `IOperatorActionRecorder.cs` / `DeadLetterRedriveResult.cs` / `OperatorActionOutcomes.cs` / `IDeadLetterRedrivePublisher.cs` / `IRequestCorrelationAccessor.cs` / `IDeadLetterRedriveService.cs` — pure shape, zero behavior.

### Layer purity (§7 / R-DOM-01 / DG-R5-01)
- Admin controllers import `Whycespace.Shared.Contracts.*` + `Whycespace.Platform.Api.*` + `Microsoft.AspNetCore.*` only — NO `Whycespace.Domain.*` imports confirmed by grep.
- `Whycespace.Api.csproj` has NO `Whycespace.Domain` project reference (pre-existing DG-R5-01 invariant preserved).
- Runtime admin services reference only `Whycespace.Shared.Contracts.*` + `Whycespace.Runtime.EventFabric` + `Whycespace.Shared.Kernel.Domain` (for `IClock` / `IIdGenerator`).

### Policy gating (§8)
- Every admin controller inherits `AdminControllerBase` which carries `[Authorize(Policy = AdminScope.PolicyName)]`. No admin endpoint bypasses this — architecture test pins it.
- Admin policy registered exactly once via `AdminAuthorizationModule.AddAdminAuthorization()`. No reuse by non-admin controllers.

### Events (§10)
- Operator actions emit `OperatorActionEvent` with deterministic event id — routed via `AuditEmission` onto the dedicated `runtime-system/control-plane/operator-action` audit stream. Naming pattern `{Domain}{Action}Event` respected (`OperatorActionEvent` — subject.verb form correct for a runtime audit envelope; no aggregate-domain prefix because the envelope is runtime-owned, not domain-owned).

### Audit (§11)
- Every execution produces: `OperatorActionEvent` (evidence), routed via `IEventFabric.ProcessAuditAsync` (chain-anchor + outbox coverage inherited from event fabric contract). No silent paths.

### Composition discipline (G-COMPLOAD-*)
- G-COMPLOAD-01 (Registry Membership): `AdminCompositionModuleEntry` listed in `CompositionRegistry.Modules`.
- G-COMPLOAD-02 (Explicit Order): `Order = 6` declared, unique, non-negative.
- G-COMPLOAD-03 (Locked Execution Sequence): guard updated to include `Admin(6)`.
- G-COMPLOAD-07 (Modules Are Orchestration-Only): `Register` body contains a single delegating call to `AddAdminCompositionModule()`.

---

## 3. Test coverage check

New tests (all passing):
- `tests/unit/runtime/admin/OperatorActionAuditRecorderTests.cs` — 6 tests (routing, determinism, field validation)
- `tests/unit/runtime/admin/DeadLetterRedriveServiceTests.cs` — 5 tests (NotFound / AlreadyReprocessed / Ineligible / PublishFailed / Accepted branches)
- `tests/unit/architecture/AdminSurfaceArchitectureTests.cs` — 4 tests (route prefix / admin policy / no domain imports / composition registration)

Updated tests:
- `tests/unit/architecture/WbsmArchitectureTests.cs` — `No_direct_Kafka_publish_outside_outbox_publisher` whitelist extended with `KafkaDeadLetterRedrivePublisher.cs` (transport-tier operator re-drive, sanctioned).
- `tests/unit/runtime/DeadLetterStoreContractTests.cs` — in-memory test double extended with `ListAllAsync`.

Final test-run summary (filter Architecture|Admin): **77 passed / 0 failed**.

Pre-existing failures unrelated to R4.B scope:
- `PolicyArtifactCoverageTests` — 8 failures, revenue/payout rego files missing from `infrastructure/policy/`. Pre-existing drift, NOT introduced by R4.B.

---

## 4. Drift / new-rules capture

No new drift rules required. All discovered discipline items were promoted directly into the canonical `runtime.guard.md` §R4.B block during execution, with the locked-sequence extension reflected in `infrastructure.guard.md` G-COMPLOAD-03.

---

## 5. Files modified / created

### Shared contracts (9 files, all new)
- `src/shared/contracts/runtime/admin/AdminScope.cs`
- `src/shared/contracts/runtime/admin/OperatorActionEvent.cs`
- `src/shared/contracts/runtime/admin/IOperatorActionRecorder.cs`
- `src/shared/contracts/runtime/admin/IDeadLetterRedriveService.cs`
- `src/shared/contracts/runtime/admin/DeadLetterRedriveResult.cs`
- `src/shared/contracts/runtime/admin/IDeadLetterRedrivePublisher.cs`
- `src/shared/contracts/runtime/admin/IRequestCorrelationAccessor.cs`
- `src/shared/contracts/runtime/admin/OperatorActionOutcomes.cs`

### Shared contracts (3 files, extended)
- `src/shared/contracts/infrastructure/messaging/IDeadLetterStore.cs` — `ListAllAsync`
- `src/shared/contracts/projections/integration/outbound-effect/IOutboundEffectProjectionStore.cs` — `ListByStatusAsync` + `ListAsync`
- `src/shared/contracts/projections/orchestration/workflow/IWorkflowExecutionProjectionStore.cs` — `ListByStatusAsync` + `ListAsync`

### Runtime (2 files, new)
- `src/runtime/control-plane/admin/OperatorActionAuditRecorder.cs`
- `src/runtime/control-plane/admin/DeadLetterRedriveService.cs`

### Host adapters (2 files, new)
- `src/platform/host/adapters/admin/HttpRequestCorrelationAccessor.cs`
- `src/platform/host/adapters/admin/KafkaDeadLetterRedrivePublisher.cs`

### Host adapters (3 files, extended)
- `src/platform/host/adapters/PostgresDeadLetterStore.cs` — `ListAllAsync`
- `src/platform/host/adapters/outbound-effects/InMemoryOutboundEffectProjectionStore.cs` — list queries
- `src/platform/host/adapters/InMemoryWorkflowExecutionProjectionStore.cs` — list queries

### Host authorization + composition (4 files, new)
- `src/platform/host/authorization/AdminAuthorizationHandler.cs`
- `src/platform/host/composition/infrastructure/authentication/AdminAuthorizationModule.cs`
- `src/platform/host/composition/runtime/admin/AdminCompositionModule.cs`
- `src/platform/host/composition/runtime/admin/AdminCompositionModuleEntry.cs`

### Host composition (2 files, extended)
- `src/platform/host/composition/infrastructure/InfrastructureCompositionRoot.cs` — calls `AddAdminAuthorization`
- `src/platform/host/composition/registry/CompositionRegistry.cs` — registers `AdminCompositionModuleEntry`

### API controllers (4 files, new)
- `src/platform/api/controllers/platform/admin/_shared/AdminControllerBase.cs`
- `src/platform/api/controllers/platform/admin/outbound-effects/OutboundEffectAdminController.cs`
- `src/platform/api/controllers/platform/admin/dlq/DlqAdminController.cs`
- `src/platform/api/controllers/platform/admin/workflow/WorkflowAdminController.cs`

### Tests (3 files new, 2 files extended)
- `tests/unit/runtime/admin/OperatorActionAuditRecorderTests.cs` (new)
- `tests/unit/runtime/admin/DeadLetterRedriveServiceTests.cs` (new)
- `tests/unit/architecture/AdminSurfaceArchitectureTests.cs` (new)
- `tests/unit/architecture/WbsmArchitectureTests.cs` — extended Kafka-publish whitelist
- `tests/unit/runtime/DeadLetterStoreContractTests.cs` — extended in-memory fake

### Guards (2 files, extended)
- `claude/guards/runtime.guard.md` — §R4.B Admin / Operator Control Surface (8 rules)
- `claude/guards/infrastructure.guard.md` — G-COMPLOAD-03 sequence extended for `Admin(6)`

---

## 6. Result

**STATUS: PASS** — R4.B admin/operator control surface landed inside the bounded scope, with every new rule pinned by architecture + behavior tests. No guard or audit drift captured. No R1–R3 correctness work reopened.
