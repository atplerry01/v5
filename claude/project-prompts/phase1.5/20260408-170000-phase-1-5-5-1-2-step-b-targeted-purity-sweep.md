# Phase 1.5 §5.1.2 — Step B: Targeted Purity Sweep & Root-Cause Classification

## TITLE
Phase 1.5 §5.1.2 Boundary Purity Validation — Step B targeted sweep,
root-cause analysis, and remediation design for BPV-D01 (and triage of
BPV-D02). Classification only; no source/guard/script edits.

## CLASSIFICATION
system / governance / boundary-control

## CONTEXT
Continuation of:
- [§5.1.2 opening pack](20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md)
- [§5.1.2 Step A baseline inventory](20260408-160000-phase-1-5-5-1-2-step-a-baseline-inventory.md)

Step A surfaced one S1 confirmed violation (BPV-D01) and one S2 review
item (BPV-D02). Step B closes the leak surface, classifies every
discovered usage, and produces the narrowest viable remediation design.

---

## 1. EXECUTIVE SUMMARY

Step B confirms BPV-D01 exhaustively. Twelve textual `Whycespace.Domain.`
matches exist under `src/platform/host/**`: **one is a comment** (an
intent statement, not a binding) and **eleven are real compile-time
type bindings** distributed across exactly three composition modules.
Zero leakage exists in `src/projections/**` or `src/systems/**` —
those layers are clean at both `using` and fully-qualified levels.

The eleven bindings cluster into a single canonical pattern: every one
is a `typeof(Whycespace.Domain.<…>Event)` argument passed to
`EventSchemaRegistry.Register(...)` (or a `(DomainEvents.<Event>)e`
cast inside a payload-mapper closure that originates from the same
registration site). There is no business logic, no orchestration, no
state ownership, and no aggregate invariant in any of the eleven sites.
The leak class is uniformly **schema identity binding** — host code is
naming domain event CLR types so the runtime event fabric can persist,
replay, and publish them.

Root cause: Whycespace v3.5 has no canonical mechanism for a domain
assembly to *contribute* its own schema descriptor without going
through host bootstrap modules. The current `IDomainBootstrapModule`
pattern lives in `src/platform/host/composition/**` and is the only
seam where event CLR types meet the schema registry. Because the
seam is on the host side, the typing must also live on the host
side — and `using Whycespace.Domain.*` is mechanically caught by
both DG-R5-HOST-DOMAIN-FORBIDDEN and runtime-guard rule
**R-DOM-01 (S1)**. The eleven sites bypass the mechanical check
*only* by writing fully-qualified type names, which both rules fail
to grep for. Rule wording is therefore the **mechanical** root cause;
the **structural** root cause is that schema identity contribution
has no domain-side or shared-contracts seam.

A canon contradiction was incidentally discovered: the comment block
in [ConstitutionalPolicyBootstrap.cs:17-18](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L17-L18)
asserts that `src/platform/host/composition/**` is a *“canonical exempt
zone for domain-typed wiring (per rule 11.R-DOM-01)”*. The actual rule
text in [runtime.guard.md:40-44](claude/guards/runtime.guard.md#L40)
contains **no such exemption**: it forbids domain-typed references
across all of `src/platform/host/**` at S1, with no composition-root
carve-out. This contradicts both the comment and the implementation
and is captured below as a `claude/new-rules/` candidate.

BPV-D02 (`systems/downstream` → `systems/midstream` workflow-name
constant) is reclassified as **NEEDS CANONICAL OWNERSHIP DECISION**;
not yet a violation, but cannot remain unowned through Phase 2.

Recommended advance state: **IN PROGRESS** with a four-file remediation
patch list ready for a future Step C execution pass.

---

## 2. BPV-D01 DETAILED FINDINGS

Twelve textual matches under `src/platform/host/**`. One is the
intent-comment in `RuntimeComposition.cs`; the other eleven are real
type bindings. The complete table:

| # | File | Line | Symbol / Type Used | Purpose | Severity | Recommendation |
|---|------|------|--------------------|---------|----------|----------------|
| 1 | [composition/runtime/RuntimeComposition.cs](src/platform/host/composition/runtime/RuntimeComposition.cs#L80) | 80 | (comment) `// runtime middleware cannot reference Whycespace.Domain.*` | Intent statement; not a binding. | none | KEEP — this comment is the canonical intent and reinforces the rule. |
| 2 | [composition/operational/sandbox/todo/TodoBootstrap.cs](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs#L93) | 93 | `typeof(Whycespace.Domain.OperationalSystem.Sandbox.Todo.TodoCreatedEvent)` (×2 — stored type + inbound type) | `EventSchemaRegistry.Register("TodoCreatedEvent", …)` — schema identity binding. | **S1** | REFACTOR — relocate to a domain-side schema descriptor (see §6 Option C). |
| 3 | TodoBootstrap.cs | 98 | `typeof(Whycespace.Domain.…Todo.TodoUpdatedEvent)` (×2) | `Register("TodoUpdatedEvent", …)` — schema identity binding. | **S1** | REFACTOR — same. |
| 4 | TodoBootstrap.cs | 103 | `typeof(Whycespace.Domain.…Todo.TodoCompletedEvent)` (×2) | `Register("TodoCompletedEvent", …)` — schema identity binding. | **S1** | REFACTOR — same. |
| 5 | TodoBootstrap.cs | 110 | `(Whycespace.Domain.…Todo.TodoCreatedEvent)e` | Cast inside `RegisterPayloadMapper` closure for outbound payload mapping. | **S1** | REFACTOR — payload mapper belongs on the domain side alongside the descriptor. |
| 6 | TodoBootstrap.cs | 115 | `(Whycespace.Domain.…Todo.TodoUpdatedEvent)e` | Same — payload-mapper closure. | **S1** | REFACTOR — same. |
| 7 | TodoBootstrap.cs | 120 | `(Whycespace.Domain.…Todo.TodoCompletedEvent)e` | Same — payload-mapper closure. | **S1** | REFACTOR — same. |
| 8 | [composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L32) | 32 | `typeof(Whycespace.Domain.ConstitutionalSystem.Policy.Decision.PolicyEvaluatedEvent)` | `Register("PolicyEvaluatedEvent", stored, …)` — stored type. | **S1** | REFACTOR — relocate to domain-side schema descriptor. |
| 9 | ConstitutionalPolicyBootstrap.cs | 33 | `typeof(Whycespace.Domain.…PolicyEvaluatedEvent)` | Same call, inbound type slot (stored == inbound; no schema contract yet). | **S1** | REFACTOR — same. Also flag: stored == inbound is a schema-contract gap. |
| 10 | ConstitutionalPolicyBootstrap.cs | 38 | `typeof(Whycespace.Domain.…PolicyDeniedEvent)` | `Register("PolicyDeniedEvent", stored, …)`. | **S1** | REFACTOR — same. |
| 11 | ConstitutionalPolicyBootstrap.cs | 39 | `typeof(Whycespace.Domain.…PolicyDeniedEvent)` | Same call, inbound type slot. | **S1** | REFACTOR — same. |
| 12 | [composition/orchestration/workflow/WorkflowExecutionBootstrap.cs](src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs#L11) | 11 | `using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;` | Namespace alias used 10× below: lines 37, 42, 47, 52, 57 (`typeof(DomainEvents.X)`) and lines 62, 67, 73, 78, 84 (payload-mapper casts). One alias, ten downstream usages. | **S1** | REFACTOR — relocate the entire schema-registration block to a domain-side descriptor. |

### Purpose taxonomy (every site)

| Purpose category | Sites | Count |
|---|---|---|
| Schema identity binding (`typeof` → `EventSchemaRegistry.Register`) | TodoBootstrap 93, 98, 103 (×2 each = 6); ConstitutionalPolicyBootstrap 32–33, 38–39 (4); WorkflowExecutionBootstrap 37, 42, 47, 52, 57 (5) | **15** |
| Outbound payload-mapper cast (`(DomainEvent)e` inside `RegisterPayloadMapper`) | TodoBootstrap 110, 115, 120 (3); WorkflowExecutionBootstrap 62, 67, 73, 78, 84 (5) | **8** |
| Namespace-alias declaration | WorkflowExecutionBootstrap 11 | **1** |
| Bootstrap metadata | (none) | 0 |
| Convenience coupling | (none) | 0 |
| True forbidden ownership (state, business rule, orchestration) | (none) | 0 |

**Aggregate severity ruling.** Per [runtime.guard.md R-DOM-01 (S1)](claude/guards/runtime.guard.md#L40)
and per the *intent* (not the literal grep predicate) of
[DG-R5-HOST-DOMAIN-FORBIDDEN](claude/guards/dependency-graph.guard.md#L217),
all eleven binding sites are **S1 architectural violations**. None
escalate to S0 because:
- No state is owned in host.
- No business rule executes in host.
- No orchestration logic lives in host.
- The bindings serve only schema identity registration — a contribution-seam concern.

### Finding 12.B — guard mechanical bypass

The mechanical predicates of `DG-R5-HOST-DOMAIN-FORBIDDEN` clause #2
(*"Contain `using Whycespace.Domain.*;` in any `*.cs` file"*) and of
`R-DOM-01` (*"`using Whycespace.Domain.*` for any
classification/context/domain"*) only catch **`using` directives**.
Fully-qualified type references (`typeof(Whycespace.Domain.X.Y)`,
`(Whycespace.Domain.X.Y)e`) and namespace aliases (`using DomainEvents
= Whycespace.Domain.X.Y`) are not matched. This is a guard hole and is
the reason §5.1.1 PASS verification missed eleven live bindings.

---

## 3. ADDITIONAL LEAKAGE FINDINGS IN PROJECTIONS OR SYSTEMS

| Tree | Predicate | Result |
|---|---|---|
| `src/projections/**` | `Whycespace\.Domain\.` (fully-qualified) | **0 matches** |
| `src/projections/**` | `Whyce\.Domain\.` (alt namespace form) | **0 matches** |
| `src/systems/**` | `Whycespace\.Domain\.` | **0 matches** |
| `src/systems/**` | `Whyce\.Domain\.` | **0 matches** |

Both layers are clean at both the `using` and fully-qualified levels.
Step A's PASS rulings on B7 (projections boundary purity) and B11
(domain ownership leakage) hold for these trees with no Step B
amendments.

---

## 4. BPV-D02 REVIEW RESULT

**File:** [src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs:3](src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs#L3)
**Coupling:** `using Whyce.Systems.Midstream.Wss.Workflows.Todo;` →
consumed symbol is `TodoLifecycleWorkflow.CreateWorkflowName` (a
string constant) at line 21:
`_workflowDispatcher.StartWorkflowAsync(TodoLifecycleWorkflow.CreateWorkflowName, intent, TodoRoute);`.

**Nature of the coupling:**
- Read-only consumption of a `const string` — no behavior crosses the seam.
- No type construction, no method invocation against midstream objects.
- The downstream handler dispatches via `IWorkflowDispatcher` (a
  shared-contracts interface), not via any midstream service.

**Classification:** **NEEDS CANONICAL OWNERSHIP DECISION.** The coupling
is benign in *behavior* but real in *namespace*: `systems/downstream`
holds a hard reference to `systems/midstream` symbols, which (a) does
not exist in reverse, and (b) is the kind of seam that grows
unobserved. Three viable resolutions:

| Option | Move target | Pros | Cons |
|---|---|---|---|
| D02-A | `Shared.Contracts.Application.Todo` (next to `CreateTodoIntent`) | Both tiers depend only on shared contracts. Canonical. | One additional contract surface; trivial cost. |
| D02-B | `Shared.Contracts.Workflow.Names` (cross-domain workflow-name registry) | Generalizes the pattern; future workflows benefit. | Premature abstraction; only one consumer today. |
| D02-C | Leave in midstream; declare downstream→midstream as a documented allowed edge | Zero code change. | Sets a precedent that downstream may bind to midstream symbols; weakens the systems-tier seam. |

**Recommendation:** Option **D02-A** during Step C. Lowest blast radius,
no precedent risk, no premature abstraction. Not yet a violation; do
not raise severity above the existing S2 review classification until
the canonical owner rules.

---

## 5. ROOT CAUSE ANALYSIS

### 5.1 Mechanical root cause
Both `R-DOM-01` (runtime.guard.md) and `DG-R5-HOST-DOMAIN-FORBIDDEN`
(dependency-graph.guard.md) encode their host→domain prohibition as
a `using`-line grep predicate. Fully-qualified type expressions and
`using X = Whycespace.Domain.…` namespace aliases bypass both
predicates. The eleven leaking sites use exactly these two bypass
forms — the leak survived §5.1.1 closure not because the rule was
absent but because the rule's predicate was incomplete.

### 5.2 Structural root cause
The `IDomainBootstrapModule` pattern places event-schema registration
inside `src/platform/host/composition/**`, which is host-side. The
schema registry (`EventSchemaRegistry`) lives in
`src/runtime/event-fabric/`, and the CLR types it indexes live in
`src/domain/**`. There is currently **no domain-side or
shared-contracts-side seam** through which a domain may contribute
its own `(name, version, storedType, inboundType, payloadMapper)`
descriptor without the registering code physically residing in
`src/platform/host/**`. As long as that seam does not exist, every
new per-domain bootstrap module will reproduce BPV-D01.

### 5.3 Canon contradiction
[ConstitutionalPolicyBootstrap.cs:17-18](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L17)
documents *“Sits in src/platform/host/composition/** which is the
canonical exempt zone for domain-typed wiring (per rule
11.R-DOM-01).”* — but rule 11.R-DOM-01 contains **no
composition-root exemption**. The exemption is folklore inside a
source-file comment and contradicts the canonical guard text. This is
a **`claude/new-rules/` capture candidate** per $1c (drift between
implementation comment and canonical rule); the proposed rule is to
either (a) make the exemption real and bounded, or (b) delete the
misleading comment after Step C remediates the leak. The repo's
authoritative answer must come from the canonical reviewer.

---

## 6. PROPOSED REMEDIATION PLAN

Three options ranked by blast radius (smallest first). Recommendation
follows.

### Option C-1 — Domain-side schema modules (recommended)

Each leaking domain gets a small `*SchemaModule.cs` file under
`src/domain/{classification}/{context}/{domain}/` (or under the
nearest existing per-domain folder) implementing a new contract
**`ISchemaModule`** declared in
`src/shared/contracts/event-fabric/ISchemaModule.cs`:

```
public interface ISchemaModule
{
    void Register(EventSchemaRegistry schema);
}
```

The `EventSchemaRegistry` type itself stays in `src/runtime/event-fabric/`
(as today). `ISchemaModule` lives in shared/contracts so domain
assemblies may implement it without taking a runtime reference (the
registry parameter is observed as an interface or as a delegate; if
the registry is a concrete class, the interface signature accepts a
shared-contracts façade `IEventSchemaSink` which the runtime registry
implements).

Each per-domain schema module relocates the affected `Register(...)`
calls and the payload-mapper closures into the domain assembly. Host
bootstrap modules then call `schemaModule.Register(schema)` on a
discovered `ISchemaModule` instance. Discovery is either:
- (i) explicit assembly scan over `Whycespace.Domain` (canonical),
- (ii) explicit list inside each per-domain bootstrap module that
      `new`s the descriptor by reflection
      (`Activator.CreateInstance(Type.GetType("Whycespace.Domain.…"))`).

Form (i) is preferred — zero domain-symbol typing in host, single
discovery seam, deterministic ordering enforceable.

After Option C-1, the eleven host-side leaks become zero. Host
bootstraps still own DI registration, projection registration, engine
registration, and workflow registration; they no longer own schema
identity binding for domain events.

### Option C-2 — Move per-domain bootstrap modules out of host

Relocate `TodoBootstrap.cs`, `ConstitutionalPolicyBootstrap.cs`, and
`WorkflowExecutionBootstrap.cs` out of `src/platform/host/composition/**`
into a sibling project (e.g., `src/platform/host.modules/` or
`src/composition/`) that legitimately holds direct references to
`Whycespace.Domain`. Host references the new project for
`IDomainBootstrapModule` discovery.

Pros: zero code change inside the modules themselves.
Cons: net-new csproj; broadens scope beyond §5.1.2; introduces a new
guard surface; conflicts with the §5.1.1 closure note that explicitly
lists this kind of extraction as a Phase 2 candidate, not a §5.1.x
item.

### Option C-3 — String-based type lookup
Replace every `typeof(Whycespace.Domain.X.Y)` with
`Type.GetType("Whycespace.Domain.X.Y, Whycespace.Domain")`. Mechanically
removes the compile-time binding.

Pros: smallest possible diff.
Cons: brittle (rename-unsafe), defeats compile-time verification,
hides the structural problem instead of fixing it, fails the spirit of
both R-DOM-01 and DG-R5-HOST-DOMAIN-FORBIDDEN.

### Recommendation
**Option C-1.** It is the only option that resolves the *structural*
root cause (no domain-side schema seam) rather than just the
*mechanical* one. C-2 is deferred to Phase 2 per §5.1.1 closure
guidance. C-3 is rejected as anti-canonical.

### Companion guard work (deferred to a separate Step C-G prompt)
- Strengthen `R-DOM-01` and `DG-R5-HOST-DOMAIN-FORBIDDEN` predicates
  to also match fully-qualified `Whycespace\.Domain\.` and
  `using\s+\w+\s*=\s*Whycespace\.Domain\.` patterns. Capture as a
  `claude/new-rules/{YYYYMMDD-HHMMSS}-guards.md` entry per $1c.
- Resolve the canon contradiction at
  [ConstitutionalPolicyBootstrap.cs:17-18](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L17):
  delete the misleading comment after Step C, or formalize the
  exemption in `runtime.guard.md` if the canonical reviewer rules
  that composition modules are in fact exempt.

---

## 7. MINIMAL FILE SET LIKELY TO CHANGE (Step C scope estimate)

Under Option C-1:

**Add (5):**
- `src/shared/contracts/event-fabric/ISchemaModule.cs` — new interface (or `IEventSchemaSink` if a façade is required to keep runtime out of shared/contracts).
- `src/domain/operational-system/sandbox/todo/TodoSchemaModule.cs` — relocates the 6 TodoBootstrap binding sites.
- `src/domain/constitutional-system/policy/decision/PolicyDecisionSchemaModule.cs` — relocates the 4 ConstitutionalPolicyBootstrap binding sites.
- `src/domain/orchestration-system/workflow/execution/WorkflowExecutionSchemaModule.cs` — relocates the 11 WorkflowExecutionBootstrap binding sites (alias + 10 references).
- (Optional) `src/runtime/event-fabric/EventSchemaSinkAdapter.cs` — concrete `IEventSchemaSink` over `EventSchemaRegistry` if interface form is chosen.

**Modify (3):**
- [src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs](src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs) — replace `RegisterSchema` body with a one-line dispatch to `TodoSchemaModule.Register(schema)`; remove the 6 fully-qualified bindings and the 3 payload-mapper casts.
- [src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs) — replace `RegisterSchema` body; delete the misleading 11.R-DOM-01 comment block on lines 17-18.
- [src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs](src/platform/host/composition/orchestration/workflow/WorkflowExecutionBootstrap.cs) — replace `RegisterSchema` body; remove the `using DomainEvents = …` alias on line 11.

**For BPV-D02 (Option D02-A, Step C scope):**
- Add `CreateWorkflowName` constant to `src/shared/contracts/application/todo/` (next to `CreateTodoIntent`).
- Modify [src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs](src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs) to import the constant from shared contracts.
- Modify the midstream consumer (if any) to point at the same shared contract.

**Out of Step C scope (deferred):**
- Guard predicate strengthening (Step C-G).
- Any csproj edge changes (none needed under Option C-1).
- Canon contradiction resolution at the rule level (escalation to canonical reviewer).
- BPV-D02 Options D02-B / D02-C.

**Estimated total churn:** ~5 new files, ~3 modified host files, ~1
modified systems file, ~1 new shared contract. No csproj changes. No
guard or audit artifact edits in Step C — those are gated on Step C-G
and the canonical reviewer's ruling on the comment contradiction.

---

## 8. STATUS RECOMMENDATION

**§5.1.2: IN PROGRESS.**

- Step A complete (baseline inventory).
- Step B complete (this artifact: targeted sweep, root-cause analysis, remediation design).
- BPV-D01: 11 confirmed S1 binding sites, all classified as schema identity binding, all addressable under Option C-1 with no csproj changes.
- BPV-D02: still S2 review; resolution path identified (D02-A) pending canonical ruling.
- Two new finding categories captured for future capture into `claude/new-rules/`:
  1. Guard predicates for R-DOM-01 / DG-R5-HOST-DOMAIN-FORBIDDEN do not match fully-qualified or aliased domain references — mechanical hole.
  2. Canon contradiction between [ConstitutionalPolicyBootstrap.cs:17-18](src/platform/host/composition/constitutional/policy/ConstitutionalPolicyBootstrap.cs#L17) comment and [runtime.guard.md R-DOM-01](claude/guards/runtime.guard.md#L40) text.
- No code, guard, audit, script, or README files were modified by this Step B artifact.
- Ready for Step C (execution of Option C-1 and D02-A) and Step C-G (guard predicate strengthening + new-rules capture) in subsequent prompts.

## OUT OF SCOPE
- Any source code modification.
- Any guard, audit, or script edit.
- Promotion of §5.1.2 status in README §6.0 (separate tracking-only edit).
- Phase-2 extraction of host adapters into `src/infrastructure/**`.
- §5.1.3 Canonical Documentation Alignment.
