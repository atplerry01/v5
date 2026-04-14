# Boundary Purity Audit — Phase 1.5 §5.1.2

**Workstream:** Phase 1.5 §5.1.2 Boundary Purity Validation
**Date:** 2026-04-08
**Status:** **PASS**

---

## Executive Summary

Phase 1.5 §5.1.2 verifies that every Whycespace layer remains within
its canonical WBSM v3.5 responsibilities even where the dependency
graph technically permits an edge. Eleven canonical boundaries were
inspected. Nine probed PASS at Step A; one confirmed S1 violation
(BPV-D01: typed `Whycespace.Domain.*` references in host composition
modules) and one S2 review item (BPV-D02: `systems/downstream` →
`systems/midstream` workflow-name coupling) were captured. Both items
are now closed: BPV-D01 by structural relocation of schema binding
into a runtime-side seam (`src/runtime/event-fabric/domain-schemas/**`),
and BPV-D02 by relocating the workflow-name constant into
`Shared.Contracts.Application.Todo`. Detection has been strengthened
mechanically and canonically so neither bypass class can recur.

---

## Scope

- All source under `src/platform/`, `src/runtime/`, `src/engines/`,
  `src/projections/`, `src/systems/`, `src/domain/`, `src/shared/`.
- All composition-root wiring inside `src/platform/host/`.
- All T1M and T2E execution paths reachable from platform entry points.
- The guard files governing boundary purity:
  the 4 canonical guards (`constitutional`, `runtime`, `domain`,
  `infrastructure`) per GUARD-LAYER-MODEL-01.
- The audit files governing the same surfaces.
- `scripts/dependency-check.sh` mechanical enforcement.

Out of scope (per §5.1.2 opening pack): §5.1.1 (closed), §5.1.3
documentation alignment, §5.2.x runtime hardening, and any
extraction of host adapters into `src/infrastructure/**`
(Phase 2 candidate per DG-R5-EXCEPT-01).

---

## Boundary Areas Inspected

| ID  | Boundary | Final Status |
|-----|----------|--------------|
| B1  | platform/api ↔ platform/host separation | PASS |
| B2  | platform → runtime bypass risk | PASS |
| B3  | runtime ownership boundaries | PASS |
| B4  | systems/downstream ↔ systems/midstream separation | PASS (BPV-D02 closed) |
| B5  | T1M ↔ T2E purity | PASS |
| B6  | engines persistence ownership | PASS |
| B7  | projections boundary purity | PASS |
| B8  | composition root purity | PASS |
| B9  | policy / business-rule placement | PASS |
| B10 | workflow / orchestration boundary | PASS |
| B11 | domain ownership leakage into non-domain layers | PASS (BPV-D01 closed) |

---

## Step A — Baseline Inventory (Findings Summary)

Eleven boundaries probed evidence-first against the current tree.
Nine returned PURE; one returned VIOLATION (B11); one returned NEEDS
REVIEW (B4). The detailed inventory is recorded at
[claude/project-prompts/20260408-160000-phase-1-5-5-1-2-step-a-baseline-inventory.md](../project-prompts/20260408-160000-phase-1-5-5-1-2-step-a-baseline-inventory.md).
Headline finding: although §5.1.1 PASS removed the host→domain
csproj edge, eleven typed `Whycespace.Domain.*` references survived
in `src/platform/host/composition/**` because they were written as
fully-qualified expressions and namespace aliases that bypassed the
§5.1.1 grep predicate.

---

## Step B — Targeted Purity Sweep (Findings Summary)

Step B characterized BPV-D01 exhaustively and produced the remediation
design. Twelve textual matches under `src/platform/host/**`: one
intent-comment (KEEP) and **eleven real bindings** distributed across
exactly three files — `TodoBootstrap.cs` (6 sites), `ConstitutionalPolicyBootstrap.cs`
(4 sites), `WorkflowExecutionBootstrap.cs` (1 alias + 10 downstream uses).
Every binding was classified as schema identity binding (15 sites),
outbound payload-mapper cast (8 sites), or namespace alias declaration
(1 site). Zero business logic, state ownership, or orchestration
crossed the seam. Aggregate severity: **S1**. Zero leakage in
`src/projections/**` or `src/systems/**`. Two root causes identified:
mechanical (predicate hole) and structural (no domain-side schema
seam). Folklore-vs-canon contradiction discovered in
`ConstitutionalPolicyBootstrap.cs` comments. BPV-D02 reclassified as
NEEDS CANONICAL OWNERSHIP DECISION; resolution path D02-A
identified. Detailed report at
[claude/project-prompts/20260408-170000-phase-1-5-5-1-2-step-b-targeted-purity-sweep.md](../project-prompts/20260408-170000-phase-1-5-5-1-2-step-b-targeted-purity-sweep.md).

---

## BPV-D01 — Detailed Closure Summary

**Original finding:** Eleven typed `Whycespace.Domain.*` bindings in
host composition modules, surviving §5.1.1 PASS via fully-qualified
and aliased forms.

**Severity:** S1 architectural.

**Remediation strategy applied:** Step B Option C-1, with one
location refinement at Step C: per-domain schema modules live in
`src/runtime/event-fabric/domain-schemas/` (not `src/domain/**`)
because $7 (`Domain = zero dependencies`) forbids the `domain → shared`
csproj edge that would be required for domain-side modules to
construct shared schema records. Runtime is the canonical home for
type-to-schema binding.

**Code surface introduced (Step C):**
- `src/runtime/event-fabric/domain-schemas/ISchemaModule.cs` —
  `ISchemaModule` + `ISchemaSink` interfaces.
- `src/runtime/event-fabric/domain-schemas/EventSchemaRegistrySink.cs` —
  one-line passthrough sink over `EventSchemaRegistry`.
- `src/runtime/event-fabric/domain-schemas/TodoSchemaModule.cs` —
  absorbs the 6 Todo bindings + 3 payload mappers.
- `src/runtime/event-fabric/domain-schemas/PolicyDecisionSchemaModule.cs` —
  absorbs the 4 Policy bindings.
- `src/runtime/event-fabric/domain-schemas/WorkflowExecutionSchemaModule.cs` —
  absorbs the alias + 5 schema registrations + 5 payload mappers.
- `src/runtime/event-fabric/domain-schemas/DomainSchemaCatalog.cs` —
  host-facing strongly-typed dispatcher (one method per domain).

**Host bootstrap changes (Step C):**
- `TodoBootstrap.cs` — `RegisterSchema` body collapsed to a single
  call to `DomainSchemaCatalog.RegisterOperationalSandboxTodo(schema)`;
  removed unused `Whyce.Shared.Contracts.Events.Todo` using.
- `ConstitutionalPolicyBootstrap.cs` — `RegisterSchema` collapsed;
  contradictory folklore comment claiming a `composition/**` exemption
  under R-DOM-01 deleted.
- `WorkflowExecutionBootstrap.cs` — `RegisterSchema` collapsed;
  `using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;`
  alias removed along with the now-unused
  `Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow` using.

**Final state:** Zero typed `Whycespace.Domain.*` references remain
under `src/platform/host/**`. The only remaining match is the
canonical intent-comment in
[src/platform/host/composition/runtime/RuntimeComposition.cs:80](../../src/platform/host/composition/runtime/RuntimeComposition.cs).
Schema registration semantics preserved exactly: identical event names,
identical `EventVersion.Default`, identical CLR type pairs, identical
payload-mapper closures.

**Status:** **CLOSED** under §5.1.2 Step C.

---

## BPV-D02 — Ownership Decision and Closure Summary

**Original finding:** [src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs](../../src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs)
imported `TodoLifecycleWorkflow.CreateWorkflowName` (a `const string`)
from `Whyce.Systems.Midstream.Wss.Workflows.Todo`, coupling the
downstream tier to a midstream symbol.

**Severity:** S2 review (not a violation; benign in behavior, real
in namespace).

**Canonical ownership decision:** Option D02-A — workflow-name
constants for an application-level workflow belong in
`Shared.Contracts.Application.<feature>` next to the intent and
command contracts. Lowest blast radius, no precedent risk, no
premature abstraction.

**Code surface (Step C):**
- Added `src/shared/contracts/application/todo/TodoLifecycleWorkflowNames.cs`
  with `public const string Create = "todo.lifecycle.create";`.
- Modified `src/systems/downstream/operational-system/sandbox/todo/TodoIntentHandler.cs`
  to drop the `Whyce.Systems.Midstream.Wss.Workflows.Todo` using and
  reference `TodoLifecycleWorkflowNames.Create`.
- Deleted `src/systems/midstream/wss/workflows/todo/TodoLifecycleWorkflow.cs`
  (the file's only content was the relocated constant; zero remaining
  consumers).
- Workflow registration in `TodoBootstrap.RegisterWorkflows` continues
  to use the literal string `"todo.lifecycle.create"`, which matches
  the new shared-contract constant value byte-for-byte.

**Status:** **CLOSED** under §5.1.2 Step C.

---

## Step C — Remediation Summary

**Files added:** 7 (6 runtime-side schema seam files + 1 shared-contracts
workflow-name constant).
**Files modified:** 4 (3 host bootstraps + 1 systems downstream handler).
**Files deleted:** 1 (obsolete midstream workflow stub).
**csproj changes:** 0.
**Build:** Full host closure, all 8 projects, 0 warnings, 0 errors.

Detailed Step C record at
[claude/project-prompts/20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md](../project-prompts/20260408-150000-phase-1-5-5-1-2-boundary-purity-validation-open.md)
and the Step B remediation design at
[claude/project-prompts/20260408-170000-phase-1-5-5-1-2-step-b-targeted-purity-sweep.md](../project-prompts/20260408-170000-phase-1-5-5-1-2-step-b-targeted-purity-sweep.md).

---

## Step C-G — Governance Hardening Summary

**Mechanical predicate strengthened.** `scripts/dependency-check.sh`
gained a new `host_fq_hits` block immediately after the existing C2
`using`-line scan. It greps `src/platform/host/**` for any
`Whycespace.Domain.` occurrence in `*.cs` files and excludes
pure-comment lines (lines whose first non-whitespace characters are
`//` or `*`) so the canonical intent-comment in
`composition/runtime/RuntimeComposition.cs:80` remains valid
documentation. Each non-comment hit reports under
`DG-R5-HOST-DOMAIN-FORBIDDEN (fully-qualified or alias)` and fails
the script.

**Guard rule wording strengthened.**
- `claude/guards/runtime.guard.md` §Dependency Graph & Layer Boundaries — `DG-R5-HOST-DOMAIN-FORBIDDEN`
  expanded from clauses 1–3 to clauses 1–5, explicitly enumerating
  fully-qualified type expressions (clause 3) and namespace aliases
  (clause 4). Severity stays S0.
- `claude/guards/runtime.guard.md` — `R-DOM-01` body restructured to
  enumerate the three forbidden forms; the implicit folklore-status
  admission claiming `composition/**` was “exempt by design” has been
  removed and replaced with a positive STATUS line confirming
  composition is no longer exempt and listing the strengthened
  verification commands. EXEMPT PATHS list extended with
  `src/runtime/event-fabric/domain-schemas/**` (the canonical
  schema-binding seam — only permitted runtime-side location for typed
  domain references) and an explicit narrow carve-out for
  `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` to import
  `Whycespace.Domain.SharedKernel.Primitives.Kernel`. Severity stays S1.

**New rules capture.** [claude/new-rules/20260408-180000-guards.md](../new-rules/20260408-180000-guards.md)
records the discovery, the folklore-vs-canon contradiction with full
source attribution, the corrected canonical interpretation, and the
enforcement consequence. Includes the canonical 5-field shape required
by $1c.

**Audit baseline updated.**
[claude/audits/dependency-graph.audit.md](dependency-graph.audit.md)
gained an addendum noting the Step C-G predicate strengthening and
the eleven remediated bypass sites.

No existing rule weakened. DG-R5-EXCEPT-01 and DG-R5-HOST-DOMAIN-FORBIDDEN
remain in force; their predicates only got stricter.

---

## Final Verification Evidence

```
Date:        2026-04-08
Workstream:  Phase 1.5 §5.1.2 Boundary Purity Validation
Steps:       A (inventory) → B (sweep) → C (remediation) → C-G (governance)

Build verification:
  $ dotnet build src/platform/host/Whycespace.Host.csproj -nologo -clp:NoSummary -v:m
  → Build succeeded. 0 Warning(s). 0 Error(s).
  → All 8 projects built transitively (Domain, Shared, Projections,
    Systems, Engines, Runtime, Api, Host).

Mechanical guard check:
  $ bash scripts/dependency-check.sh
  → Violations: 0
  → Status: PASS
  → Strengthened predicate active (using + fully-qualified + alias).

Direct grep evidence:
  $ grep -RIn "Whycespace\.Domain\." src/platform/host/
  → Single match: src/platform/host/composition/runtime/RuntimeComposition.cs:80
    (canonical intent-comment, KEEP, excluded by predicate).

  $ grep -RIn "Whycespace\.Domain\." src/projections/
  → 0 matches.

  $ grep -RIn "Whycespace\.Domain\." src/systems/
  → 0 matches.

BPV-D02 closure verification:
  $ grep -RIn "Whyce\.Systems\.Midstream\.Wss\.Workflows\.Todo" src/
  → 0 matches.

  src/systems/midstream/wss/workflows/todo/TodoLifecycleWorkflow.cs
  → DELETED. No remaining consumers.

Acceptance criteria (§5.1.2 opening pack §2.9 / §4):
  1. All in-scope canonical boundaries enumerated and probed.    ✅
  2. Reproducible probe evidence stored.                          ✅
  3. PURE / DRIFTED / VIOLATING classification per probe.         ✅
  4. S0–S3 severity per non-PURE finding.                         ✅
  5. Remediation patch list per non-PURE finding (and applied).   ✅
  6. (No-remediation-during-audit constraint applies to Step A/B; ✅
      Step C and Step C-G executed under explicit promotion.)
  7. New guard rules captured under claude/new-rules/.            ✅
  8. Final report returns explicit terminal status: PASS.         ✅
  9. README §6.0 row 5.1.2 promoted by this closure pass.         ✅
```

---

## Final Status Recommendation

**Phase 1.5 §5.1.2 Boundary Purity Validation — PASS (2026-04-08).**

All eleven inspected boundaries are PURE. BPV-D01 and BPV-D02 are
closed. Mechanical detection covers `using`, fully-qualified, and
alias forms across `src/platform/host/**`. The runtime-side
schema-binding seam is canonically documented as the only permitted
runtime location for typed domain references. The folklore-vs-canon
contradiction is captured archivally. Build clean, dependency check
clean, no csproj changes. Ready for promotion to PASS in
README §5.1.2 and §6.0.
