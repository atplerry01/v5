# Phase 1.5 §5.1.2 — Step A: Baseline Boundary Inventory

## TITLE
Phase 1.5 §5.1.2 Boundary Purity Validation — Step A baseline boundary
inventory and initial drift list.

## CLASSIFICATION
system / governance / boundary-control

## CONTEXT
Continuation of the §5.1.2 workstream opening pack at
[20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md](20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md).
§5.1.1 PASS established at 2026-04-08 (host → domain csproj edge
removed, dependency-graph clean). §5.1.2 Step A is inventory and
classification only — no source/guard/audit/script/README modifications.

---

## 1. EXECUTIVE SUMMARY

Step A probed eleven canonical WBSM v3.5 boundaries against the current
tree. Nine boundaries probe **PASS**, one probes **VIOLATION**, and one
probes **NEEDS REVIEW**. The headline finding is that although §5.1.1
removed the *direct* `<ProjectReference>` from `Whycespace.Host` to
`Whycespace.Domain`, three composition modules under
`src/platform/host/composition/**` still contain **fully-qualified
typed references to `Whycespace.Domain.*`** which compile only via
*transitive* domain access through `Whycespace.Engines`,
`Whycespace.Runtime`, and `Whycespace.Projections` (each of which does
hold a direct domain reference). This is exactly the class of drift
§5.1.2 was opened to detect: the dependency graph is technically clean,
but the boundary it was meant to enforce is semantically pierced.

A second, lower-severity item is a `systems/downstream` →
`systems/midstream` import in the Todo intent handler, which couples
the downstream tier to a midstream workflow-name constant. This needs
canonical review to determine whether constant-name ownership is
permissible across system tiers or whether the constant must move into
`shared/contracts`.

All other probes (platform/api purity, T1M↔T2E purity, engines
persistence ownership, projections boundary, runtime determinism,
runtime persistence ownership) returned PURE evidence.

Recommended advance state: **IN PROGRESS** with one S1 violation, one
S2 review item, and one tightly scoped Step B sweep.

---

## 2. BOUNDARY INVENTORY TABLE

| ID  | Boundary | Canonical Rule | Observed State | Status | Evidence | Notes |
|-----|----------|----------------|----------------|--------|----------|-------|
| B1  | platform/api ↔ platform/host separation | `platform/api` is request surface only; must not reference host concerns; may consume only `Whyce.Shared.Contracts.*` and `Whyce.Shared.Kernel.*`. | All 4 inspected `using Whyce.*` lines under `src/platform/api/**` reference only `Shared.Contracts.*` and `Shared.Kernel.*`. Zero references to Runtime/Engines/Domain/Projections/Systems/Host. | PASS | [src/platform/api/controllers/TodoController.cs](src/platform/api/controllers/TodoController.cs) · [src/platform/api/middleware/ConcurrencyConflictExceptionHandler.cs](src/platform/api/middleware/ConcurrencyConflictExceptionHandler.cs) · [src/platform/api/health/HealthAggregator.cs](src/platform/api/health/HealthAggregator.cs) | API surface is contract-only. |
| B2  | platform → runtime bypass risk | Platform code may not invoke runtime internals that bypass canonical execution flow; runtime entry must be via `Shared.Contracts.Runtime` interfaces. | API references runtime exclusively through `Whyce.Shared.Contracts.Runtime`. Host references concrete runtime namespaces (`Whyce.Runtime.EventFabric`, `Whyce.Runtime.Projection`, `Whyce.Runtime.ControlPlane`, `Whyce.Runtime.Dispatcher`, `Whyce.Runtime.Middleware.*`, `Whyce.Runtime.Topology`) only inside `composition/**` and `adapters/**` — composition-root scope under DG-R5-EXCEPT-01. | PASS | [src/platform/host/composition/runtime/RuntimeComposition.cs](src/platform/host/composition/runtime/RuntimeComposition.cs) · [src/platform/host/composition/projections/ProjectionComposition.cs](src/platform/host/composition/projections/ProjectionComposition.cs) | No bypass from `platform/api`; host concrete-runtime references are bounded to composition + adapter scope. |
| B3  | runtime ownership boundaries | Only runtime persists, publishes, or anchors. Engines must remain stateless. | No `DbContext`, `Npgsql`, `IDbConnection`, `SqlConnection`, `IProducer`, or `IConsumer` references inside `src/runtime/**`. Only mention of `Guid.NewGuid` / `DateTime.UtcNow` is the comment block in [src/runtime/guards/DeterminismGuard.cs](src/runtime/guards/DeterminismGuard.cs). Persistence is implemented in `src/platform/host/adapters/**` against runtime contracts (`PostgresEventStoreAdapter`, `PostgresProjectionWriter`, `PostgresOutboxAdapter`, `KafkaOutboxPublisher`). | PASS | `src/platform/host/adapters/*` · `src/runtime/guards/DeterminismGuard.cs` | Persistence ownership lives at the canonical seam: runtime defines the contract, host adapters implement it. |
| B4  | systems/downstream ↔ systems/midstream separation | Downstream and midstream must not silently couple; cross-tier coupling must flow through shared contracts. | `src/systems/midstream/**` has zero `using Whyce.Systems.Downstream.*`. `src/systems/downstream/**` has **one** `using Whyce.Systems.Midstream.*` in [TodoIntentHandler.cs:3](src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs#L3) consuming `TodoLifecycleWorkflow.CreateWorkflowName` (a string constant). | NEEDS REVIEW | [src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs:3](src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs#L3) | Coupling is name-only, not behavior; canonical reviewer must decide whether the constant moves to `Shared.Contracts.*` or whether downstream is permitted to reference midstream workflow names. |
| B5  | T1M ↔ T2E purity | T1M (machine) and T2E (event) tiers must remain non-bypassable; no direct cross-tier source coupling. | `src/engines/T1M/**` has zero `using Whyce.Engines.T2E.*`. `src/engines/T2E/**` has zero `using Whyce.Engines.T1M.*`. | PASS | `src/engines/T1M/**` · `src/engines/T2E/**` | T1M and T2E are source-clean. Behavioral non-bypass remains a runtime-flow check (out of Step A scope). |
| B6  | Engines persistence ownership | Engines must be stateless; no persistence types, no clock, no RNG. | No persistence types in `src/engines/**`. Only `DateTime.UtcNow` / `Guid.NewGuid` mentions are *comments* in [WorkflowExecutionReplayService.cs:22](src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs#L22) and [PolicyDecisionEventFactory.cs:16](src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs#L16) explicitly asserting purity. | PASS | `src/engines/T1M/lifecycle/WorkflowExecutionReplayService.cs` · `src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs` | Engines remain pure per $7 / $9. |
| B7  | Projections boundary purity | Projections may consume events only; must not reference Runtime/Engines/Domain/Systems/Platform internals. | `src/projections/**` contains zero `using Whyce.Runtime.*`, zero `using Whyce.Domain.*`, zero `using Whyce.Engines.*`, zero `using Whyce.Systems.*`, zero `using Whyce.Platform.*`. | PASS | `src/projections/**` | Reaffirms §5.1.1 D-R7-01 closure at the using-level. |
| B8  | Composition root purity | `platform/host` may wire dependencies but must not execute business logic, route commands, or own state. | All concrete runtime / engines / systems / projections imports under `src/platform/host/**` are confined to `composition/**`, `bootstrap/**`, and `adapters/**`. No business invariants observed in composition modules; modules only register services and map names → types. | PASS | [src/platform/host/composition/registry/CompositionRegistry.cs](src/platform/host/composition/registry/CompositionRegistry.cs) · [src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs) | Composition modules are wiring-only; no command routing or business rule observed in host. |
| B9  | Policy / business-rule placement | Policy and business invariants live in domain (rules) and runtime (enforcement); never in host or projections. | No business invariants observed in host composition modules or in projections. Policy decision construction is in [src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs](src/engines/T0U/whycepolicy/PolicyDecisionEventFactory.cs) (engine-pure factory) and policy enforcement is in `src/runtime/middleware/**`. | PASS | `src/runtime/middleware/**` · `src/engines/T0U/whycepolicy/**` | No silent migration of business rules into the wrong layer detected at Step A depth. |
| B10 | Workflow / orchestration boundary | Orchestration belongs in runtime (`runtime/workflow`, `runtime/dispatcher`); engines provide steps; host provides composition only. | Workflow registry lives at [src/runtime/workflow/WorkflowRegistry.cs](src/runtime/workflow/WorkflowRegistry.cs); dispatchers at [src/runtime/dispatcher/RuntimeCommandDispatcher.cs](src/runtime/dispatcher/RuntimeCommandDispatcher.cs) and [src/runtime/dispatcher/SystemIntentDispatcher.cs](src/runtime/dispatcher/SystemIntentDispatcher.cs). Engine T1M provides step types (`ValidateIntentStep`, `CreateTodoStep`, `EmitCompletionStep`). Host registers workflow definitions in `TodoBootstrap.RegisterWorkflows`. No orchestration logic observed inside host beyond name-to-type wiring. | PASS | `src/runtime/workflow/**` · `src/runtime/dispatcher/**` · `src/engines/T1M/steps/**` | Orchestration ownership is canonically placed. |
| B11 | Domain ownership leakage into non-domain layers | No layer outside `src/domain/**` and the domain's direct consumers (engines, runtime) may declare typed references to `Whycespace.Domain.*`. Host explicitly removed its csproj edge under §5.1.1; therefore no host source file may use a `Whycespace.Domain.*` type. | **VIOLATION.** Eleven typed `Whycespace.Domain.*` references survive in `src/platform/host/composition/**`, distributed across three files. They compile only because `Whycespace.Engines`, `Whycespace.Runtime`, and `Whycespace.Projections` each hold a direct csproj reference to `Whycespace.Domain`, transitively re-exposing domain types to the host assembly. The §5.1.1 closure removed the *edge* but not the *usage*. | VIOLATION | [src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs:93](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L93), [:98](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L98), [:103](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L103), [:110](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L110), [:115](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L115), [:120](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L120) · [src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs:32-33](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L32-L33), [:38-39](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L38-L39) · [src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs:11](src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs#L11) | Internal evidence: [src/platform/host/composition/runtime/RuntimeComposition.cs:80](src/platform/host/composition/runtime/RuntimeComposition.cs#L80) explicitly comments `runtime middleware cannot reference Whycespace.Domain.*`, confirming the canonical intent that the host code violates. |

---

## 3. DRIFT LIST

| Drift ID | Boundary | Title | Severity | Type | Files | One-line |
|---|---|---|---|---|---|---|
| BPV-D01 | B11 | Host composition modules use typed `Whycespace.Domain.*` references via transitive csproj graph | **S1** (architectural) | CONFIRMED VIOLATION | `src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs` (6 sites), `src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs` (4 sites), `src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs` (1 alias) | Host pierces the §5.1.1 host→domain boundary semantically even though the csproj edge is gone; transitive access via `Whycespace.Engines` / `Whycespace.Runtime` / `Whycespace.Projections` re-exposes domain types. |
| BPV-D02 | B4 | `systems/downstream` references a `systems/midstream` workflow-name constant | **S2** (structural) | REVIEW | [src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs:3](src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs#L3) | Coupling is name-only (`TodoLifecycleWorkflow.CreateWorkflowName`) but it crosses the systems-tier seam; canonical reviewer must rule on whether workflow-name constants must live in `Shared.Contracts.*`. |

**Confirmed Violations:** BPV-D01.
**Review Items:** BPV-D02.

No S0 (system-breaking) findings at Step A depth.

---

## 4. HIGH-RISK AREAS

- **`src/platform/host/composition/**` typed-domain leakage** — BPV-D01 directly invalidates the spirit of §5.1.1. The §5.1.1 PASS evidence verified there are no `using Whycespace.Domain.*` lines, but these usages are written as fully-qualified type names (no `using` directive), so the §5.1.1 grep predicate did not catch them. This is the highest-priority Step B target.
- **Transitive domain re-export from `Whycespace.Engines`, `Whycespace.Runtime`, `Whycespace.Projections`** — these three projects all hold a direct `<ProjectReference>` to `Whycespace.Domain`. .NET project transitivity therefore re-exposes `Whycespace.Domain.*` to every assembly that references any of them, including `Whycespace.Host`. The dependency-graph guard does not currently model transitive exposure.
- **Cross-tier systems coupling at the name-constant level** — BPV-D02 is small but is the kind of seam that typically grows. Worth a canonical decision before Phase 2 expansion.

---

## 5. RECOMMENDED STEP B SWEEP

Narrowest, highest-value next sweep:

1. **B-1 Domain-leakage exhaustive sweep.** Run a fully-qualified-name probe (not just `using` lines) for `Whycespace.Domain\.` against `src/platform/host/**`, `src/projections/**`, and `src/systems/**`. Capture every site verbatim. Expected output: a complete list of typed-domain leak sites that BPV-D01 must remediate.
2. **B-2 Transitive-exposure model.** Audit `claude/guards/dependency-graph.guard.md` and `scripts/dependency-check.sh` for whether they model *transitive* domain re-export. If they do not, capture the gap as a `claude/new-rules/` candidate per $1c (do not modify guards yet).
3. **B-3 Workflow-name ownership review.** Targeted canonical review of `TodoLifecycleWorkflow.CreateWorkflowName` placement: domain (no), engines (no), runtime (yes if dispatch-related), midstream (current), or `Shared.Contracts.*` (likely correct). Capture the decision; do not move code yet.
4. **B-4 Composition-root behavior probe.** Read each `composition/**/*Bootstrap.cs` end-to-end to confirm none execute business logic beyond DI registration, schema mapping, and projection wiring. Step A reviewed `TodoBootstrap.cs` and found wiring-only; the other bootstraps (constitutional policy, workflow execution) need the same line-by-line read.

Out of Step B scope: any remediation patch, any guard or audit edit, any csproj change, any source rewrite. Step B remains classification-only.

---

## 6. INITIAL STATUS RECOMMENDATION FOR §5.1.2

**IN PROGRESS** — workstream has executed Step A successfully and produced evidence-backed findings. Two drift items captured (one S1 confirmed violation, one S2 review item). Step B sweep is scoped and ready to execute. No blockers. README §6.0 row 5.1.2 may be advanced from `NOT STARTED` to `IN PROGRESS` in a follow-up tracking-only edit; this Step A artifact does not modify README per the opening-pack discipline.

---

## OUT OF SCOPE
- Any code remediation for BPV-D01 or BPV-D02 (deferred to a later step).
- Any guard or audit file modification.
- Any csproj or script modification.
- Promotion of §5.1.2 status in README §6.0 (separate tracking-only edit).
- §5.1.3 Canonical Documentation Alignment.
