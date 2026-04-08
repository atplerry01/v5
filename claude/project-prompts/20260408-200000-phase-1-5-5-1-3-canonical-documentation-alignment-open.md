# Phase 1.5 §5.1.3 — Canonical Documentation Alignment (Workstream Opening Pack)

## TITLE
Phase 1.5 §5.1.3 Canonical Documentation Alignment — canonical workstream opening pack.

## CLASSIFICATION
system / governance / documentation-alignment

## CONTEXT
Phase 1.5 §5.1.1 Dependency Graph Remediation closed PASS on 2026-04-08
(see [20260408-143500-phase-1-5-5-1-1-pass-closure.md](20260408-143500-phase-1-5-5-1-1-pass-closure.md)).
Phase 1.5 §5.1.2 Boundary Purity Validation closed PASS on 2026-04-08
(see [20260408-190000-phase-1-5-5-1-2-pass-closure.md](20260408-190000-phase-1-5-5-1-2-pass-closure.md)).
The dependency graph and boundary purity are now correct *in the code*;
the next structural risk is **canon-to-implementation drift in the
documentation surface itself**: README, guards, audits, project
prompts, rule captures, and implementation-adjacent comments that may
no longer accurately reflect the repository or the locked WBSM v3.5
architecture.

§5.1.2 already surfaced a concrete instance of exactly this drift
class (the folklore comment in `ConstitutionalPolicyBootstrap.cs`
claiming a `composition/**` exemption that the canonical rule body
never granted). That single discovery is sufficient evidence that a
broader, systematic alignment pass is the next mandatory Phase 1.5
structural hardening item before Phase 2.

This artifact is the **opening pack only**. No remediation work is
performed here. No source, guard, audit, script, or README file is
modified. The workstream is created in `OPEN` state and handed off
for execution in subsequent prompts.

---

## 1. EXECUTIVE SUMMARY

§5.1.3 Canonical Documentation Alignment verifies that every
documentation surface in the repository — README, `claude/guards/**`,
`claude/audits/**`, `claude/project-prompts/**`, `claude/new-rules/**`,
and implementation-adjacent comments under `src/**` — accurately
reflects (a) the actual state of the codebase and (b) the locked
WBSM v3.5 canonical architecture. Where §5.1.1 asked *“is this edge
allowed?”* and §5.1.2 asked *“is the code on this edge doing only
what its layer is allowed to do?”*, §5.1.3 asks *“does what we say
about the system actually match what the system is?”*.

The workstream produces three classifications per documentation
artifact: **CURRENT TRUTH** (load-bearing for present decisions —
must be accurate), **HISTORICAL BASELINE** (frozen-in-time evidence
of a past state — must be clearly marked as such and not presented
as current), and **ARCHIVAL RECORD** (closed prompts, captured
rules, completed audits — preserved verbatim, never edited except
for typos). Drift is any artifact that claims to be the first
category but is actually one of the other two, or any artifact whose
content contradicts canon, the current code, or another artifact.

The §5.1.2 folklore-vs-canon discovery is the prototype for this
class of drift. §5.1.3 will systematically sweep the repository for
the same pattern, classify every documentation artifact, produce a
drift register, remediate the live drift, and tighten the discipline
boundary between current/historical/archival so future work cannot
silently slide between categories. Initial status: **OPEN**.

---

## 2. WORKSTREAM DEFINITION

### 2.1 Purpose
Verify that canonical documentation, guards, audits, project prompts,
rule captures, and implementation-adjacent comments accurately reflect
the actual repository state and the locked WBSM v3.5 architecture, and
that the discipline boundary between current truth, historical
baseline, and archival record is enforced and visible.

### 2.2 Objective
Produce an evidence-backed determination, per documentation artifact
in scope, of whether the artifact is **ALIGNED**, **STALE**,
**CONTRADICTED**, or **MIS-CATEGORIZED** (claims to be current but
is actually historical), and convert every non-ALIGNED finding into
a tracked remediation item with severity, owner, and acceptance
test.

### 2.3 Why This Matters Before Phase 2
- Phase 2 expansion will be planned, scoped, and gated against the
  documentation surface that §5.1.3 audits. If the README master
  tracking table, the guard rules, or the audit baselines are
  misaligned with reality, Phase 2 planning will optimize against a
  fictional state and will produce wrong gates.
- §5.1.2 proved that documentation drift is a *load-bearing* defect:
  the folklore comment in `ConstitutionalPolicyBootstrap.cs` directly
  caused a structural violation to survive §5.1.1 PASS verification
  for an additional cycle. Documentation drift is not cosmetic.
- Future agents (and future humans) will read these artifacts as
  ground truth. A guard rule that says one thing while the script
  enforces another is an invitation to silently regress.
- The §5.1.x structural hardening series is meant to leave Phase 1.5
  with a clean, self-consistent canonical surface. §5.1.3 is the
  closing act of that series and is the precondition for §5.2.x
  operational certification.
- Historical baselines that are not clearly marked as such tend to
  be re-cited as current truth in later prompts, which is how
  drift compounds.

### 2.4 Known Documentation-Drift Risk Areas
- **D1** — README §5.1.x workstream entries vs. actual workstream
  state (now that 5.1.1 and 5.1.2 are PASS, are the surrounding
  sections still consistent?).
- **D2** — README §6.0 master tracking table vs. individual
  workstream sections (rows must agree with section bodies).
- **D3** — `claude/guards/**` rule bodies vs. the mechanical checks
  in `scripts/dependency-check.sh` (rule wording must match what is
  actually enforced; this is the §5.1.2 Step C-G class of drift).
- **D4** — `claude/guards/**` rule bodies vs. each other (e.g.
  `dependency-graph.guard.md` and `runtime.guard.md` must agree
  on the host→domain prohibition; §5.1.2 found a contradiction here).
- **D5** — `claude/audits/**` baseline blocks marked as “current”
  that are actually frozen evidence from a prior pass.
- **D6** — `claude/project-prompts/**` closure prompts whose claims
  no longer match the post-closure repository state (e.g. a closure
  note that lists files that have since been moved or deleted).
- **D7** — `claude/new-rules/**` captures that have been promoted
  into canonical guards but whose archival record incorrectly
  implies the rule is still pending promotion.
- **D8** — Folklore comments in `src/**` source files that cite
  non-existent canonical exemptions, non-existent rule numbers,
  or contradicted rule bodies (the §5.1.2 prototype).
- **D9** — `CLAUDE.md` execution rules vs. the actual `claude/`
  directory layout and guard/audit file inventory.
- **D10** — Project-prompt cross-references (e.g. one prompt
  citing another by path) where the cited path has been renamed
  or the cited content has been edited.
- **D11** — Stale TODO/FIXME/“phase Bxx”-style markers in code
  comments that refer to phases that have since closed.
- **D12** — Discipline-boundary violations: artifacts that
  intermix current truth, historical baseline, and archival record
  without clear separation, making it impossible for a future
  reader to distinguish.

### 2.5 Scope
- `README.md` — every section, with particular attention to §5.1.x,
  §5.2.x, §5.3.x, §5.4.x, §5.5.x workstream entries and the
  §6.0 master tracking table.
- `CLAUDE.md` — execution rules vs. actual `claude/` directory state.
- `claude/guards/**/*.guard.md` — rule bodies, examples, status
  footers, EXEMPT PATHS lists, mechanical-check assertions.
- `claude/audits/**/*.audit.md` — baseline blocks, status lines,
  evidence claims, “current” markers.
- `claude/audits/**/*.audit.output.md` — frozen audit outputs;
  classification check (must be HISTORICAL BASELINE or ARCHIVAL
  RECORD, never CURRENT TRUTH unless explicitly so marked).
- `claude/project-prompts/**/*.md` — closure prompts and their
  claims about post-closure repository state; cross-reference
  integrity.
- `claude/new-rules/**/*.md` — promotion-status integrity
  (captures whose rules have been promoted vs. those still pending).
- `src/**/*.cs` comments containing the strings `canonical`, `canon`,
  `per rule`, `R-DOM`, `DG-R`, `phase`, `Phase`, `TODO`, `FIXME`,
  `exempt`, `forbidden`, or any explicit guard-rule citation —
  inspected for folklore-vs-canon contradictions.
- `docs/**` if any documentation lives there and bears on canonical
  rules.

### 2.6 Non-Scope
- §5.1.1 (closed) and §5.1.2 (closed) re-verification beyond
  inspecting their closure prompts for documentation drift.
- §5.2.x runtime infrastructure hardening.
- §5.3.x performance, §5.4.x security, §5.5.x governance.
- Refactoring documentation structure or directory layout.
- Rewriting historical baselines into current-truth statements
  (historical baselines must remain frozen).
- Any change to DG-R5-EXCEPT-01, DG-R5-HOST-DOMAIN-FORBIDDEN, or
  R-DOM-01 wording beyond the predicate-strengthening already
  applied in §5.1.2 Step C-G.
- Tooling work to automate documentation linting (Phase 2
  candidate; this workstream is a one-shot manual alignment pass).

### 2.7 Remediation Strategy
1. **Inventory** — enumerate every documentation artifact in scope
   and classify each as CURRENT TRUTH, HISTORICAL BASELINE, or
   ARCHIVAL RECORD by **intent** (what the artifact claims to be).
2. **Probe** — for each CURRENT TRUTH artifact, run targeted checks
   against the actual repository / actual rule body / actual code
   to confirm the claim. For each HISTORICAL BASELINE / ARCHIVAL
   RECORD, confirm it is clearly marked as such and is not being
   re-cited as current.
3. **Classify** — every probe result is `ALIGNED`, `STALE` (claim
   was once true but is no longer), `CONTRADICTED` (claim is in
   active conflict with another canonical source), or
   `MIS-CATEGORIZED` (artifact is presented as current truth but
   is actually historical).
4. **Triage** — assign severity per WBSM v3 §16: S0 system-breaking,
   S1 architectural, S2 structural, S3 formatting / cosmetic.
5. **Patch list** — non-ALIGNED findings become a remediation
   patch list with explicit acceptance tests; no inline edits during
   the audit pass itself.
6. **Discipline-boundary tightening** — every documentation artifact
   that intermixes current/historical/archival without clear
   separation is flagged with a structural fix (header banner,
   explicit “HISTORICAL — frozen 2026-04-XX” marker, or relocation
   to an `_archives/` subtree).
7. **Promote** — execution and remediation occur in follow-up
   prompts; this opening pack ends at the patch-list handoff.

### 2.8 Task Breakdown
- **T-A** Documentation inventory — enumerate every artifact in
  §2.5 scope; classify each by stated intent (CURRENT TRUTH /
  HISTORICAL BASELINE / ARCHIVAL RECORD).
- **T-B** Probe matrix — define one or more probes per artifact
  category and per drift risk area (D1–D12), each with an exact
  command, grep predicate, or cross-reference check.
- **T-C** Probe execution — run the probe matrix against the
  current tree; capture verbatim raw output as evidence.
- **T-D** Classification — assign `ALIGNED` / `STALE` /
  `CONTRADICTED` / `MIS-CATEGORIZED` to every probe result with a
  one-line justification.
- **T-E** Severity triage — assign S0/S1/S2/S3 to every non-ALIGNED
  finding per $16.
- **T-F** Patch list — produce remediation patch list with file
  paths, intended edit, and acceptance test per item.
- **T-G** Folklore-comment sweep — targeted scan of `src/**` for
  comments citing canonical rules, exemptions, or phase markers;
  cross-check each citation against the cited canonical source.
- **T-H** Discipline-boundary structural recommendations — for each
  artifact that intermixes current/historical/archival, propose the
  minimal structural fix (banner, marker, relocation).
- **T-I** Final canonical-alignment audit report — single artifact
  bundling inventory, probe matrix, raw evidence, classifications,
  severities, patch list, structural recommendations, and PASS/FAIL
  determination.

### 2.9 Acceptance Criteria
1. Every documentation artifact in §2.5 scope is enumerated in the
   inventory and classified by stated intent (CURRENT TRUTH /
   HISTORICAL BASELINE / ARCHIVAL RECORD).
2. Every artifact classified as CURRENT TRUTH has at least one
   reproducible probe verifying its claim against the actual
   repository state.
3. Every probe has reproducible evidence (command + raw output)
   stored alongside the audit report.
4. Every probe result is classified as ALIGNED / STALE /
   CONTRADICTED / MIS-CATEGORIZED with a one-line justification.
5. Every non-ALIGNED finding has S0–S3 severity assigned per $16.
6. Every non-ALIGNED finding has a remediation patch list entry
   with file path, intended change, and acceptance test.
7. Folklore-comment sweep (T-G) covers every `src/**/*.cs` file
   containing any of the search strings in §2.5; every cited
   canonical rule is cross-checked against its actual source.
8. The discipline boundary between current truth, historical
   baseline, and archival record is explicitly stated for every
   artifact category, and every intermixing case is flagged.
9. No remediation patch is applied during the audit pass; opening
   pack discipline is preserved until §5.1.3 advances out of the
   audit phase.
10. Any newly discovered guard rule or governance finding is
    captured under `claude/new-rules/` with the canonical 5-field
    shape per $1c.
11. Final report explicitly returns one of: `PASS`, `FAIL`,
    `PARTIAL`, `BLOCKED`, `WAIVED`, with the reason recorded.
12. The §5.1.3 row in README §6.0 is updated only when the
    workstream actually advances state — not by the opening pack
    itself.

### 2.10 Evidence Required
- Documentation inventory table with stated-intent classification.
- Probe matrix (probe ID, artifact ID, drift risk ID,
  command/predicate, expected ALIGNED shape).
- Raw probe output for every probe (verbatim).
- Classification table (probe ID → ALIGNED / STALE / CONTRADICTED /
  MIS-CATEGORIZED + reason).
- Severity table (finding ID → S0/S1/S2/S3).
- Folklore-comment sweep raw results with per-citation verdict.
- Remediation patch list.
- Discipline-boundary structural recommendation list.
- New-rules capture file (if any).
- Final canonical-alignment audit report with explicit terminal
  status.

---

## 3. TRACKING TABLE

| Field | Value |
|---|---|
| **ID** | 5.1.3 |
| **Topic** | Canonical Documentation Alignment |
| **Objective** | Verify that every documentation surface in scope (README, CLAUDE.md, guards, audits, project prompts, new-rules captures, in-source comments) accurately reflects the actual repository state and the locked WBSM v3.5 canon, and that current truth / historical baseline / archival record discipline is visible and enforced. |
| **Tasks** | T-A Inventory · T-B Probe matrix · T-C Probe execution · T-D Classification · T-E Severity triage · T-F Patch list · T-G Folklore-comment sweep · T-H Discipline-boundary recommendations · T-I Final audit report |
| **Deliverables** | Documentation inventory · Probe matrix · Raw probe evidence · Classification table · Severity table · Folklore-comment sweep results · Remediation patch list · Discipline-boundary structural recommendations · New-rules capture (if any) · Final canonical-alignment audit report |
| **Evidence Required** | Reproducible probe commands and raw output for every CURRENT TRUTH artifact in §2.5; classification + severity for every finding; folklore-sweep verdict per cited rule; explicit terminal status (PASS/FAIL/PARTIAL/BLOCKED/WAIVED) |
| **Status** | OPEN (NOT STARTED — workstream defined, no execution yet) |
| **Risk** | MEDIUM-HIGH — §5.1.2 already proved this class of drift is load-bearing (the folklore comment caused a structural violation to survive §5.1.1 verification). Latent S1/S2 findings are very plausible across the README, guard bodies, and project-prompt closure claims. |
| **Blockers** | None known. §5.1.1 PASS and §5.1.2 PASS prerequisites both satisfied 2026-04-08. |
| **Owner** | Whycespace governance / structural hardening track |
| **Notes** | Opening pack only. No remediation in this prompt. Distinguishes carefully between CURRENT TRUTH (must be accurate), HISTORICAL BASELINE (frozen, marked), and ARCHIVAL RECORD (preserved verbatim). The §5.1.2 folklore-vs-canon discovery is the prototype drift instance and should be used as a worked example in the Step A inventory. Promotion of state in README §6.0 occurs only when execution advances. Continuity with §5.1.1 PASS (2026-04-08) and §5.1.2 PASS (2026-04-08) preserved. |

**Status legend:** NOT STARTED · IN PROGRESS · PARTIAL · BLOCKED · PASS · FAIL · WAIVED.

---

## 4. ACCEPTANCE CRITERIA
(See §2.9 above. Reproduced here for tracking convenience.)

1. Every in-scope documentation artifact enumerated and intent-classified.
2. Every CURRENT TRUTH artifact has at least one reproducible probe.
3. Every probe has reproducible raw evidence.
4. Every probe result classified ALIGNED / STALE / CONTRADICTED / MIS-CATEGORIZED with reason.
5. Every non-ALIGNED finding has S0–S3 severity.
6. Every non-ALIGNED finding has a remediation patch list entry with acceptance test.
7. Folklore-comment sweep covers every `src/**/*.cs` matching the search strings; every citation cross-checked.
8. Discipline-boundary intermixing flagged with a structural fix.
9. No remediation applied during audit pass.
10. Any newly discovered guard rule captured under `claude/new-rules/`.
11. Final report returns explicit terminal status.
12. README §6.0 row 5.1.3 updated only on real state change.

---

## 5. REQUIRED ARTIFACTS

- `claude/project-prompts/20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md`
  — this opening pack.
- `claude/audits/canonical-alignment.audit.md` — to be created during
  T-I (final audit report). Not created by this opening pack.
- `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` — to be created
  during T-G or T-H if and only if newly discovered governance
  rules emerge.
- README §5.1.3 (existing) — unchanged by this opening pack; the
  workstream definition is anchored here, but state promotion is
  gated on real execution.
- README §6.0 master tracking table row 5.1.3 — unchanged by this
  opening pack.

---

## 6. CLAUDE EXECUTION PROMPT

> **Use this prompt to execute §5.1.3 in a follow-up session. Do not
> execute it as part of this opening pack.**

```
Phase 1.5 §5.1.3 — Canonical Documentation Alignment (Execution Pass)

CLASSIFICATION: system / governance / documentation-alignment
CONTEXT: §5.1.1 PASS (2026-04-08); §5.1.2 PASS (2026-04-08).
  Opening pack:
  claude/project-prompts/20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md

OBJECTIVE: Execute T-A through T-I of §5.1.3 as defined in the opening
  pack. Produce claude/audits/canonical-alignment.audit.md as the
  single consolidated deliverable. Do not modify source, guards,
  scripts, or README outside the audit artifact and (if needed) one
  or more claude/new-rules/ capture files.

CONSTRAINTS:
  - WBSM v3 canonical execution rules ($1–$16) apply in full.
  - Pre-execution: load every guard in claude/guards/ ($1a). No skip,
    no cache, no summary.
  - Post-execution: run every audit in claude/audits/ ($1b). Inline-fix
    any drift discovered against the audit artifact itself before
    completion.
  - Anti-drift ($5): no architecture changes, no renames, no file
    moves, no inference of missing components.
  - File system ($6): only operate in /src, /infrastructure, /tests,
    /docs, /scripts, /claude.
  - No remediation patches applied; produce the patch list only.
  - Any newly discovered guard rule → claude/new-rules/ per $1c
    (do not silently extend an existing guard).
  - Drift risk areas: D1–D12 from §2.4 of the opening pack.
  - Distinguish strictly between CURRENT TRUTH, HISTORICAL BASELINE,
    and ARCHIVAL RECORD on every artifact.

EXECUTION STEPS:
  1. T-A Inventory — enumerate every artifact in §2.5 scope and
     classify by stated intent.
  2. T-B Probe matrix — define probes (commands or grep predicates)
     for each artifact category and each drift risk area D1–D12.
     Each probe must declare its expected ALIGNED shape.
  3. T-C Probe execution — run every probe; capture verbatim raw
     output.
  4. T-D Classification — ALIGNED / STALE / CONTRADICTED /
     MIS-CATEGORIZED per probe with one-line justification.
  5. T-E Severity triage — S0/S1/S2/S3 for every non-ALIGNED finding
     per $16.
  6. T-F Patch list — file path + intended edit + acceptance test
     per non-ALIGNED finding. No edits applied.
  7. T-G Folklore-comment sweep — targeted scan of src/**/*.cs for
     the §2.5 search strings; per-citation verdict against the
     cited canonical source.
  8. T-H Discipline-boundary recommendations — flag every artifact
     that intermixes current/historical/archival; propose minimal
     structural fix per case.
  9. T-I Final report — write claude/audits/canonical-alignment.audit.md
     bundling inventory, probe matrix, raw evidence, classifications,
     severities, folklore-sweep results, patch list, discipline-
     boundary recommendations, and explicit terminal status.

OUTPUT FORMAT:
  - Single audit artifact: claude/audits/canonical-alignment.audit.md
  - Optional: claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md
  - Structured failure report on any halt ($12: STATUS / STAGE /
    REASON / ACTION_REQUIRED).

VALIDATION CRITERIA:
  - All twelve acceptance criteria from §2.9 / §4 satisfied.
  - Terminal status one of: PASS / FAIL / PARTIAL / BLOCKED / WAIVED.
  - Audit sweep ($1b) clean against the produced artifact.
  - No source/guard/script/README modification outside the explicitly
    named artifacts.
```

---

## 7. INITIAL STATUS

**OPEN** — workstream defined, tracked, and ready for execution. No
remediation performed. No source, guard, audit, script, or README
file modified by this opening pack. §5.1.3 enters the Phase 1.5 work
queue as the next mandatory structural hardening item after §5.1.1
PASS (2026-04-08) and §5.1.2 PASS (2026-04-08), and is the closing
act of the §5.1.x structural hardening series before §5.2.x
operational certification begins.
