# PHASE 2 ARCHITECTURE PASS (PROMPT B — DRAFT)

```
STATUS:        DRAFT — REQUIRES EXPLICIT $5 WAIVER BEFORE EXECUTION
CLASSIFICATION: system / phase2-architecture / cross-cutting
PRECEDED BY:    20260407-160500-system-phase2-hardening-bugfix.md (commit 1e990c6)
FOLLOWED BY:    20260407-XXXXXX-governance-rule-promotion.md (Prompt C, after B lands)
```

## CONTEXT

Prompt A (bugfix subset) landed cleanly and passed full audit sweep. This
prompt covers the architectural changes that were carved out of the
original Phase 2 mega-prompt because they violate $5 Anti-Drift on the
default constraint set.

Each step here is a real architectural change with non-trivial blast
radius. They MUST NOT be executed under the standard $5 rule. They
require an explicit per-step waiver, design review, and per-step
verification.

## OBJECTIVE

Bring runtime/projection/host composition to the next architectural
maturity level: shared contracts for projection handlers, reflection-loaded
host composition root, runtime control-plane indirection through systems,
and domain naming cleanup.

## REQUIRED WAIVER (must be granted before execution)

```
$5 ANTI-DRIFT WAIVER
  Granted by: <user>
  Date:       <YYYY-MM-DD>
  Scope:      Steps B-1 through B-4 of this prompt only.
              No other architectural changes are authorized.
              No moves/renames outside the explicit list below.
  Conditions:
    1. Each step is its own commit.
    2. Each step's commit follows audit sweep PASS against the
       changed paths.
    3. Build green after each step. Halt on first red.
    4. Any unexpected fan-out (e.g. >5 unrelated files modified)
       triggers immediate STOP and re-confirmation.
```

---

## DROPPED FROM ORIGINAL PROMPT (do not execute)

### ~~Step 3 — EventReplayService PolicyHash preservation~~ DROPPED

The `PolicyHash="replay"`, `ExecutionHash="replay"`,
`Timestamp=DateTimeOffset.MinValue` sentinels in
[EventReplayService.cs:55-59](src/runtime/event-fabric/EventReplayService.cs#L55-L59)
are **by design**, documented in
[replay-determinism.audit.md:53-72](claude/audits/replay-determinism.audit.md#L53-L72):

> "If a future requirement needs full envelope-field equality on rebuild,
> EventReplayService would need to load the original envelopes from the
> store rather than reconstructing them. **That is a design change, not
> a fix.**"

The original prompt's instruction would have erased intentional design
markers and produced a worse system. This is an **informational
finding from the Prompt A audit sweep** ([sweeps/20260407-160500-prompt-a-bugfix.md](claude/audits/sweeps/20260407-160500-prompt-a-bugfix.md)).

If a real requirement emerges for full envelope replay equality, it must
arrive as its own design-change prompt that **first updates
`replay-determinism.audit.md`** to remove the by-design clause, then
extends `EventStoreService` to persist and return per-event envelope
metadata, then changes `EventReplayService` to use it. None of that is
in scope here.

### ~~Step 9 — Domain `Adapter*` rename~~ DEFERRED indefinitely

Renaming domain types is not architectural maturation; it's cosmetic
cleanup with high blast radius (every reference, every test, every
event schema). Defer until a domain-rename pass with its own scope and
its own waiver.

---

## STEPS

### B-1 — `IProjectionHandler` relocation to shared contracts

**Goal:** Move the envelope-based projection handler contract out of
`src/runtime/projection/` into `src/shared/contracts/projection/` so
that `src/projections/**` can implement it without referencing
`src/runtime/**`. This closes pre-existing tracked violation
**DG-R7-01** in `dependency-graph.guard.md`.

**Pre-conditions to verify (before any edit):**
1. Confirm the current location and exact namespace of `IProjectionHandler`
   in `src/runtime/projection/`.
2. Enumerate all implementers via grep (`: IProjectionHandler` and
   `EnvelopeProjectionHandler =`).
3. Enumerate all `using Whyce.Runtime.Projection;` consumers in
   `src/projections/**` and `src/platform/host/**`.
4. Verify no other type in the current file (`EventEnvelope`,
   `ProjectionExecutionPolicy`, etc.) is co-located with
   `IProjectionHandler` — if so, the move scope expands and this step
   must be re-scoped before execution.

**Edit plan:**
1. Create `src/shared/contracts/Projection/IProjectionHandler.cs` with the
   identical contract. Same namespace path
   `Whyce.Shared.Contracts.Infrastructure.Projection` (already used by
   `IProjectionHandler<T>` per [TodoProjectionHandler.cs:6](src/projections/operational-system/sandbox/todo/TodoProjectionHandler.cs#L6))
   — verify the typed `IProjectionHandler<T>` and the envelope-based
   `IProjectionHandler` can coexist there.
2. Update every implementer's `using` directive.
3. Delete the original file.
4. Rebuild solution. Halt on any red.

**Risks:**
- `EventEnvelope` type itself currently lives in `Whyce.Runtime.EventFabric`.
  Moving `IProjectionHandler` into shared without also moving
  `EventEnvelope` creates a transitive dependency from `shared` →
  `runtime`, which is **forbidden**. Step B-1 may need to first move
  `EventEnvelope` into shared (or define a shared contract twin).
  **This MUST be resolved during pre-conditions, before edits.**
- The tracked violation is in `Whycespace.Projections.csproj` referencing
  `Whycespace.Runtime.csproj`. Removing that reference requires that no
  symbol from `Whyce.Runtime.*` remain in `src/projections/**` after the
  move. Pre-condition (3) above is the gate.

**Rollback:** revert the commit; the move is contained to a small set
of files.

**Audit gate:** dependency-graph.audit, structural.audit, projection.audit.

---

### B-2 — `ModuleCatalogLoader` reflection-driven host composition

**Goal:** Eliminate the host's direct compile-time references to
`runtime`, `engines`, `projections`, and `domain` (tracked violation
**DG-R5-01**) by introducing a reflection/registry-based composition
loader.

**STATUS: BLOCKED ON DESIGN.** A `ModuleCatalogLoader` is a **new
architectural pattern**, not a refactor of existing code. The
[composition-loader.guard.md](claude/guards/composition-loader.guard.md)
G-COMPLOAD-06 explicitly **forbids reflection-based discovery** in the
existing `CompositionRegistry`:

> "FAIL IF the loader, registry, or any composition module discovers
> types via reflection (`Assembly.GetTypes`, `Activator.CreateInstance`,
> attribute scans, etc.). Module enumeration is explicit list literals
> only."

A literal-list `ModuleCatalogLoader` already exists as
`CompositionModuleLoader` per
[program-composition.guard.md](claude/guards/program-composition.guard.md).
The original prompt's "ModuleCatalogLoader (reflection-based)"
instruction **directly contradicts a locked guard**.

**Required before execution:**
1. Decide whether the host's direct project references are an actual
   problem given that `BootstrapModuleCatalog` already encapsulates the
   per-domain wiring (per
   [runtime.guard.md:69-71](claude/guards/runtime.guard.md#L69-L71) Phase B2a).
2. If yes, the design must use the existing literal-list pattern, not
   reflection. Step B-2 then becomes "extend `BootstrapModuleCatalog`
   coverage so the project references can be removed", which is a
   smaller, less invasive change.
3. If the existing `BootstrapModuleCatalog` already covers it, **no
   change is needed**, and the tracked violation **DG-R5-01** should be
   resolved by either (a) granting the composition-root exception in
   `dependency-graph.guard.md` (per its own escape clause) or (b)
   migrating remaining references behind the catalog.

**Recommendation:** Defer B-2 until the design question is answered.
Most likely outcome is "grant the composition-root exception in
`dependency-graph.guard.md`" — that is a one-line guard edit, not an
architectural change.

---

### B-3 — Systems boundary tightening

**Goal:** Ensure `src/systems/**` reaches runtime only through
`IRuntimeControlPlane` / `IWorkflowDispatcher` shared-contract surfaces,
and the `TodoController` indirects through `ITodoIntentHandler` rather
than calling a dispatcher directly. This closes any remaining
**SYS-BOUND-01** violations from
[systems.guard.md](claude/guards/systems.guard.md#L155).

**Pre-conditions to verify:**
1. Does `IRuntimeControlPlane` exist in `src/shared/contracts/runtime/`?
   The original prompt assumed yes; pre-flight didn't confirm. Grep
   first.
2. Does `ITodoIntentHandler` exist? If not, defining it is a structural
   addition, not a refactor.
3. List all `src/systems/**` files that reference `ISystemIntentDispatcher`.
4. List all `src/platform/api/controllers/TodoController.cs` dispatcher
   usages.

**Edit plan (conditional on pre-conditions):**
1. If `IRuntimeControlPlane` is missing, **STOP**. Adding it is its own
   prompt with its own design review.
2. If present, replace each call site, one file per commit.
3. For the controller, only proceed if `ITodoIntentHandler` already
   exists; otherwise stop.

**Risks:**
- `ISystemIntentDispatcher` may have call patterns that
  `IRuntimeControlPlane` does not support 1-to-1. Verify surface area
  parity before any edit.
- Engine rule **E-STEP-02** (engines.guard) recently flagged that
  re-entry from step→runtime→engine via `ISystemIntentDispatcher`
  requires explicit architectural approval. If the systems layer is the
  only legitimate consumer, removing it may be safe; if T1M steps
  consume it too, this step expands.

---

### B-4 — Closeout

After B-1 (and optionally B-3 if pre-conditions hold), run:

1. Full audit sweep against changed files.
2. Build all projects (host, unit tests, integration tests).
3. Update `claude/audits/sweeps/` with the post-B sweep.
4. Surface findings into Prompt C (rule promotion).

---

## VALIDATION CRITERIA

- Each step's commit must be independently bisectable.
- `dotnet build` green after each commit.
- Audit sweep PASS against changed files after each commit.
- No new guard violations introduced.
- `dependency-graph.guard.md` violations DG-R5-01 and DG-R7-01 either
  remediated OR explicitly converted into documented exceptions.

## OUTPUT FORMAT

Per-step: file diffs, build result, audit sweep result, commit hash.

## FAILURE

Any step that fails its build or audit sweep triggers immediate halt
and structured failure report per $12. No partial-step commits.
