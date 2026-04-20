# CS.13B — Band-F Semantic README Completion (PLANNED FOLLOW-UP GATE)

Status: **PLANNED — not yet executed.**
Predecessor: CS.13A (identifier renames + AssetKind alignment) — COMPLETE.
Phase 2.6 realignment is CLOSED; this gate is a documentation-only follow-up.

---

## Hard scope boundary

This gate is **STRICTLY documentation**. It touches only `*.md` files under
`src/domain/content-system/**`. It **MUST NOT**:
- modify any `*.cs` file
- move, create, or delete any directory
- rename any identifier
- introduce, rename, or retire any aggregate / event / VO / specification / error / service
- change namespaces
- alter the 55-leaf topology
- revisit `PlaybackAggregate` naming

If any audit-driven defect requires code work, it is OUT-OF-SCOPE for CS.13B
and must be handled in a separate fix-gate. CS.13B does not reopen
realignment under any circumstances.

---

## In-scope deliverables

### 1. Leaf lifecycle / state diagrams
For every leaf that has a live aggregate (pre-existing or absorbed):
- State diagram (text form, e.g., `Draft → Active → Archived`)
- Transition arrows with the triggering command
- Terminal states clearly marked
- Rejection rules for each transition (one-line each)

Leaves in scope (aggregates live, state model exists):
- **Document core-object:** document, file, bundle, record, template
- **Document descriptor:** metadata
- **Document intake:** upload
- **Document lifecycle-change:** processing, version
- **Document governance:** retention
- **Media core-object:** asset, subtitle, transcript
- **Media descriptor:** metadata
- **Media intake:** ingest
- **Media technical-processing:** processing
- **Media lifecycle-change:** version
- **Streaming stream-core:** stream, channel, manifest, availability
- **Streaming live-streaming:** broadcast, archive
- **Streaming playback-consumption:** session
- **Streaming delivery-governance:** access, observability

Scaffolded leaves (no aggregate yet) receive ONLY a "planned state model"
paragraph — no fake diagram.

### 2. Event example lists
Per live-aggregate leaf, an exhaustive bulleted list of domain events with
a one-line purpose each. Format:

```
- {Domain}{Action}Event — fired when ...
```

### 3. Upstream / downstream reference maps
Per live-aggregate leaf, a two-section structure:

```
## Upstream references
- {domain-path}/{Aggregate} — this leaf consumes {Event|Ref} from ...

## Downstream consumers
- {domain-path}/{Aggregate} — this leaf emits {Event} consumed by ...
```

Sourced by inspecting Ref VOs and cross-leaf event observer patterns.
If no upstream/downstream consumers exist today, the section states
"No current consumers" (not an empty heading).

### 4. Group cohesion / invariant notes
Per group (18 total), extend the group README with:

- **Cohesion rationale** — one short paragraph explaining why these specific
  leaves are grouped together, framed around a shared semantic class.
- **Group-level invariants** — if any structural rules apply to the whole
  group (e.g., "every leaf must reference an AssetRef"), state them.
- **Group-level non-inclusion** — what a reader might expect but is NOT here,
  with a pointer to the correct group.

### 5. Context non-inclusion notes
Per context (4 total), extend the context README with a dedicated
"Non-inclusion" section:

- Concerns frequently conflated with this context but NOT owned here
- Explicit pointers to where those concerns are owned (either in another
  context of content-system, or outside content-system entirely)

---

## Out-of-scope (explicit)

- `PlaybackAggregate` rename — ONLY to be revisited by a separate dedicated
  inspection gate (not CS.13B). CS.13B READMEs may describe Playback's
  role in `streaming/stream-core/availability/` but must not propose or
  foreshadow a rename.
- Quality domain implementation — quality remains scaffold. README may
  describe the §CD-03 split rule and expected evaluative VOs, but no code.
- Mirror-layer documentation — CS.13B documents the domain layer only.
  Mirror-layer scaffolding is its own phase.
- New-rules captures — CS.13B does not produce new guard/audit findings.
  If a defect surfaces, it is handled as a separate fix-gate.

---

## Acceptance criteria

1. Every leaf README follows the Band-F 6-section template:
   Purpose / Owns / Does-Not-Own / Lifecycle-State / Events / Upstream-Downstream.
2. Every group README has a "Why this group exists" paragraph, group-level
   invariants (or "none — this is a grouping by semantic class only"),
   and non-inclusion pointers.
3. Every context README has a non-inclusion section.
4. `git diff --name-only` of CS.13B's commit(s) shows **only** `*.md` paths
   under `src/domain/content-system/**`.
5. `grep -E '\.cs$'` of CS.13B's file-change list returns zero lines.
6. No structural tree changes observable by `find` or `ls`.
7. No identifier changes observable by grep across the five CS.13A rename patterns.

---

## Suggested execution pattern

- **Wave A** — leaf READMEs for live-aggregate leaves (~30 leaves).
- **Wave B** — leaf READMEs for scaffold-only leaves (~25 leaves).
- **Wave C** — group READMEs (18).
- **Wave D** — context READMEs (4) + root.
- **Wave E** — audit sweep.

Estimated scale: ~60 README rewrites. No code risk. Fully reversible.

---

## Schedule / trigger

CS.13B is **planned** but not scheduled. It should run when:
- Phase 2.6 closure has been accepted (CONFIRMED).
- No higher-priority feature work is blocked by incomplete READMEs.
- A session explicitly invokes CS.13B with a go-signal.

Until invoked, the Band-S READMEs (present as of CS.13A) are sufficient for
navigation, ownership clarity, and boundary discipline.
