# Canonical Alignment Audit — Phase 1.5 §5.1.3

**Workstream:** Phase 1.5 §5.1.3 Canonical Documentation Alignment
**Date:** 2026-04-08
**Status:** **PASS**

---

## Executive Summary

Phase 1.5 §5.1.3 verifies that every implementation-adjacent
documentation surface in the repository — README, CLAUDE.md, guards,
audits, project prompts, new-rules captures, and in-source comments —
accurately reflects the actual repository state and the locked
WBSM v3.5 canonical architecture, and that the discipline boundary
between **CURRENT TRUTH**, **HISTORICAL BASELINE**, and
**ARCHIVAL RECORD** is visible and enforced.

Where §5.1.1 asked *“is this edge allowed?”* and §5.1.2 asked
*“is the code on this edge doing only what its layer is allowed to
do?”*, §5.1.3 asks *“does what we say about the system actually match
what the system is?”*. The §5.1.2 folklore-comment discovery
(`ConstitutionalPolicyBootstrap.cs` claiming a non-existent
`composition/**` exemption) was the prototype drift instance.

§5.1.3 enumerated 40 documentation artifacts, executed 22 probes
across two Step B sets, surfaced ten drift items (DOC-D01 through
DOC-D10), and closed all ten across two Step C sets. Two non-blocking
S3 review items remain explicitly deferred. Zero S0 or S1 findings.
Build, dependency check, and folklore sweep all clean.

---

## Scope

- `README.md` — every section, with particular attention to §5.1.x
  workstream entries and the §6.0 master tracking table.
- `CLAUDE.md` — execution rules and orchestration directory inventories.
- `claude/guards/**/*.guard.md` — 4 canonical guards (constitutional, runtime, domain, infrastructure) per GUARD-LAYER-MODEL-01.
- `claude/audits/**/*.audit.md` (21 active definitions + 14 frozen
  `*.audit.output.md` outputs + `phase1-evidence/` and `sweeps/`
  subtrees).
- `claude/project-prompts/**/*.md` (top-level §5.1.x prompts +
  `phase1/` and `phase2/`).
- `claude/new-rules/**/*.md` (9 active captures + `_archives/`).
- `docs/**` (operational playbook, quick reference, migrations,
  validation report, todo notes, e-series).
- `src/**/*.cs` comments matching folklore/staleness patterns
  (`Phase B*`, `TODO`, `FIXME`, `R-DOM-01`, `DG-R*`).
- `claude/` root for unclassified artifacts.

Out of scope: §5.1.1 (closed) and §5.1.2 (closed) re-verification
beyond inspecting their closure prompts; §5.2.x runtime hardening;
§5.3.x–§5.5.x; documentation linter tooling; rewriting historical
baselines into current-truth statements.

---

## Discipline Model

§5.1.3 establishes the canonical discipline model for
implementation-adjacent documentation:

- **CURRENT TRUTH (CT)** — load-bearing for present decisions; must
  be accurate; every artifact in this category must have a
  reproducible probe verifying its claim against the actual
  repository state.
- **HISTORICAL BASELINE (HB)** — frozen evidence of a past state;
  must be clearly marked (date stamp, “LOCKED”, “BASELINE”
  marker, or `_archives/` location); must NOT be re-cited as current.
- **ARCHIVAL RECORD (AR)** — closed prompts, captured rules,
  completed audits; preserved verbatim; never edited except for
  typos.

**Drift** is any artifact that claims category 1 but is actually
category 2 or 3, or whose content contradicts canon, current code,
or another canonical artifact.

The model is now embedded in Step A inventory classification,
Step B probe matrices, and the §5.1.3 closure record.

---

## Step A — Inventory Summary

Step A enumerated 40 documentation surfaces and intent-classified
each as CT / HB / AR / UC. Five immediate drift candidates surfaced
without running the full Step B matrix:

- **DOC-D01** — CLAUDE.md guard list omits 14+ active guards.
- **DOC-D02** — CLAUDE.md audit list omits 24+ active audits.
- **DOC-D03** — CLAUDE.md `claude/` directory description stale and
  internally inconsistent with $2.
- **DOC-D04** — `runtime.guard.md §Dependency Graph & Layer Boundaries` R5 body self-superseded
  by DG-R5-EXCEPT-01 in the same file but never rewritten.
- **DOC-D05** — `runtime.guard.md §Dependency Graph & Layer Boundaries` CODE-LEVEL CHECKS
  contradict DG-R5-EXCEPT-01.
- **DOC-D06** — `dependency-graph.audit.md` C2 predicate has the
  same drift class as DOC-D05.

The detailed inventory is recorded at
[claude/project-prompts/20260408-210000-phase-1-5-5-1-3-step-a-canonical-documentation-inventory.md](../project-prompts/20260408-210000-phase-1-5-5-1-3-step-a-canonical-documentation-inventory.md).

---

## Step B Set 1 — Findings Summary

Step B Set 1 executed seven load-bearing probes (CT-INV B-1..B-3,
GG-DG B-4..B-5, GG-AUD B-6..B-7). All seven returned **DRIFT** or
**CONTRADICTION** with hard counts:

- **B-1:** **[STALE 2026-04-14 — inventory superseded by 4-layer canonical model; see GUARD-LAYER-MODEL-01]** CLAUDE.md guard list = 12 files; actual = 25 top-level
  + 5 `domain-aligned/` = **30 total**. Omission rate **18 of 30 =
  60%**, including `dependency-graph`, `runtime-order`,
  `stub-detection`, `config-safety`, the entire `domain-aligned/`
  subtree.
- **B-2:** CLAUDE.md audit list = 11 files; actual = 21 active +
  14 frozen outputs. Omission rate **10 of 21 = 48%**, critically
  including `boundary-purity.audit.md` and `dependency-graph.audit.md`.
- **B-3:** CLAUDE.md `claude/` directory description omits four
  directories and contradicts $2 internally on the canonical
  prompt-store name.
- **B-4:** `runtime.guard.md §Dependency Graph & Layer Boundaries` R5 body lines 59–64 contradict
  DG-R5-EXCEPT-01 lines 164+. The same file self-acknowledges the
  contradiction at lines 186–190 but the body was never rewritten.
- **B-5:** CODE-LEVEL CHECKS lines 100–107 grep `src/platform` as a
  whole, which would fail on legitimate composition-root usings.
  The script `scripts/dependency-check.sh` correctly distinguishes
  `platform/api` from `platform/host`; the guard does not.
- **B-5b:** LOCK CONDITIONS line 150 says “All rules R1–R7 pass”
  but the rule set has expanded with DG-R5-EXCEPT-01,
  DG-R5-HOST-DOMAIN-FORBIDDEN, DG-R7-01. **DOC-D04 scope expanded
  from 1 site → 3 sites.**
- **B-6:** `dependency-graph.audit.md` C2 has the same drift as
  the guard's CODE-LEVEL CHECKS.
- **B-7:** Audit INPUTS line 11 also lists `R1–R7` only.

Detailed report at
[claude/project-prompts/20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md](../project-prompts/20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md).

---

## Step C Set 1 — Patch Summary

Five high-leverage patches applied across three files:

- **P-D01** ([CLAUDE.md](../../CLAUDE.md) lines 9–13) — replaced the
  static 12-guard list with a discovery directive: *“Load every
  `*.guard.md` file under `claude/guards/**`, including the
  `claude/guards/domain-aligned/**` subtree.”* Survives Phase 2
  guard additions.
- **P-D02** ([CLAUDE.md](../../CLAUDE.md) lines 18–22) — replaced
  the static 11-audit list with a discovery directive that also
  marks `*.audit.output.md` and the `phase1-evidence/` and `sweeps/`
  subtrees as HISTORICAL BASELINE / ARCHIVAL RECORD.
- **P-D03** ([CLAUDE.md](../../CLAUDE.md) lines 80–84) — rewrote the
  `claude/` directory description to enumerate all 12 actual
  entries; promoted `project-prompts/` as canonical per $2;
  explicitly labeled `prompts/` as a legacy supporting directory.
- **P-D04** ([runtime.guard.md §Dependency Graph & Layer Boundaries](../guards/runtime.guard.md))
  — three coordinated edits: R5 body now cross-references
  DG-R5-EXCEPT-01 and DG-R5-HOST-DOMAIN-FORBIDDEN; CODE-LEVEL CHECKS
  now name `scripts/dependency-check.sh` as authoritative and split
  the `src/platform` predicate into `platform/api` and
  `platform/host`; LOCK CONDITIONS now reference R1–R7 plus the
  DG-* additions.
- **P-D06** ([dependency-graph.audit.md](dependency-graph.audit.md))
  — PURPOSE rewritten to name the full DG-* rule set; C2 predicate
  split into `platform/api` and `platform/host` to match the script.

No rule weakened. Severities preserved. Historical context (DG-BASELINE-01,
the self-acknowledgement paragraph at lines 186–190) preserved untouched.
Detailed Step C Set 1 record at
[claude/project-prompts/20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md](../project-prompts/20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md).

---

## Step B Set 2 — Findings Summary

Step B Set 2 executed the remaining 8+ probes (GG-OTHER B-8/B-9,
NR B-10, FOL B-11/B-12, TR B-13, DOCS B-14, PP-PHASE2 B-15,
claude-root B-Root). Most returned **PASS**:

- **B-8/B-9:** GG-OTHER spot-check of `runtime`, `platform`,
  `structural`, `engine`, `policy` guards plus the `domain-aligned/`
  subtree (now consolidated into `runtime.guard.md`,
  `infrastructure.guard.md`, and `domain.guard.md` per
  GUARD-LAYER-MODEL-01) → **PASS** (no R-DOM-01-class
  folklore-status-footer drift). `runtime.guard.md §Dependency Graph
  & Layer Boundaries` remains the unique outlier (already patched).
- **B-10:** NR promotion-status integrity → **DRIFT** (3 of 9 active
  captures lack a `STATUS:` field). New finding: **DOC-D10**.
- **B-11:** FOL `Phase B*` sweep → **DRIFT** (13 sites in 9 files).
  Confirms **DOC-D07**.
- **B-11b:** TODO/FIXME sweep → **PASS** (0 hits).
- **B-12:** R-DOM-01 / DG-R* citation cross-check → **PASS** (every
  cited rule still canonical post §5.1.2 Step C-G).
- **B-13:** README §6.0 row-vs-section integrity → **PASS** (27/27
  rows match their section bodies).
- **B-14:** `docs/**` classification → **PASS** (6 of 7 ALIGNED;
  one (`e2e-validation-report.md`) self-marks as scaffold and is
  NEEDS REVIEW but non-blocking).
- **B-15:** `claude/project-prompts/phase2/` → **DRIFT** (empty
  directory exists). New finding: **DOC-D09**.
- **B-Root:** `claude/audits.zip` → **DRIFT** (133 KB, 2026-04-07,
  unclassified). New finding: **DOC-D08**.

Zero S0 or S1 findings. Detailed report at
[claude/project-prompts/20260408-230000-phase-1-5-5-1-3-step-b-set2-remaining-drift-sweep.md](../project-prompts/20260408-230000-phase-1-5-5-1-3-step-b-set2-remaining-drift-sweep.md).

---

## Step C Set 2 — Cleanup Summary

Four targeted patches applied:

- **P-D10** — added explicit `STATUS: PROPOSED` blocks to the three
  new-rules captures lacking promotion-status, with verification
  evidence recorded inline. Each STATUS line names the proposed
  destination guard. Verified by direct grep against canonical
  guards: zero matches for `ACT-FABRIC-ROUNDTRIP-TEST-01`,
  `DET-SEED-DERIVATION-01`, `E-LIFECYCLE-FACTORY-CALL-SITE-01`.
- **P-D07** — stripped or past-tense-rewrote 13 stale `Phase B2a/B2b`
  source-comment markers across 9 files in
  `src/platform/host/adapters/**`,
  `src/platform/host/composition/**`, `src/runtime/event-fabric/**`,
  and `src/runtime/projection/**`. For two summary blocks
  (`IDomainBootstrapModule.cs`, `TodoBootstrap.cs`) the rewrite
  preserves useful historical meaning by anchoring to the §5.1.2
  BPV-D01 remediation rather than the closed phase marker.
- **P-D08** — deleted `claude/audits.zip`. The 133 KB zip dated
  2026-04-07 was an export snapshot superseded by the live
  `claude/audits/**` tree. Git history retains the snapshot.
- **P-D09** — deleted the empty `claude/project-prompts/phase2/`
  directory. It will be re-created naturally when the first
  Phase 2 prompt is stored under it per $2.

Detailed Step C Set 2 record at
[claude/project-prompts/20260408-230000-phase-1-5-5-1-3-step-b-set2-remaining-drift-sweep.md](../project-prompts/20260408-230000-phase-1-5-5-1-3-step-b-set2-remaining-drift-sweep.md).

---

## Closed Drift Register

| ID | Title | Severity | Closed By |
|---|---|---|---|
| **DOC-D01** | **[STALE 2026-04-14 — inventory superseded by 4-layer canonical model; see GUARD-LAYER-MODEL-01]** CLAUDE.md guard list omits 18 of 30 actual guards | S2 | P-D01 (Step C Set 1) |
| **DOC-D02** | CLAUDE.md audit list omits 10 of 21 actual audits | S2 | P-D02 (Step C Set 1) |
| **DOC-D03** | CLAUDE.md `claude/` directory description stale + internally inconsistent with $2 | S3 | P-D03 (Step C Set 1) |
| **DOC-D04** | `runtime.guard.md §Dependency Graph & Layer Boundaries` self-supersedes itself in three places (R5 body, CODE-LEVEL CHECKS, LOCK CONDITIONS) | S2 | P-D04 (Step C Set 1) |
| **DOC-D05** | `runtime.guard.md §Dependency Graph & Layer Boundaries` CODE-LEVEL CHECKS predicate forbids legitimate composition-root usings | S2 | P-D04 (folded into Step C Set 1) |
| **DOC-D06** | `dependency-graph.audit.md` C2 predicate / INPUTS drift | S2 | P-D06 (Step C Set 1) |
| **DOC-D07** | Stale `Phase B2a/B2b` markers in 13 source-comment sites across 9 files | S3 | P-D07 (Step C Set 2) |
| **DOC-D08** | `claude/audits.zip` exists at `claude/` root with no canonical classification | S3 | P-D08 (Step C Set 2 — deleted) |
| **DOC-D09** | `claude/project-prompts/phase2/` is an empty directory | S3 | P-D09 (Step C Set 2 — deleted) |
| **DOC-D10** | Three `claude/new-rules/20260408-103326-*.md` captures lack STATUS field | S2 | P-D10 (Step C Set 2) |

**Ten drift items closed. Zero S0 or S1 findings. No rule weakened.
No runtime behavior modified.**

---

## Explicitly Deferred Non-Blocking Review Items

These items are out of scope for §5.1.3 PASS gate by design:

- **`docs/validation/e2e-validation-report.md`** — self-marks as
  *“Generated: 2026-04-08 (scaffold)”*. Discipline category UNCLEAR
  (presented as a report, contains scaffold content). Resolution
  deferred to §5.x.x validation work where the actual report
  content will be populated. **S3, non-blocking.**
- **Three prose-form new-rules captures** (`140605-infrastructure`,
  `142631-validation`, `145000-validation-live-execution`) — STATUS
  is implicit in prose rather than declared in a canonical field.
  Lower priority because the prose self-documents promotion status.
  **S3, non-blocking.**

Both items are tracked outside the §5.1.3 PASS gate and will be
addressed in their respective downstream workstreams.

---

## Final Verification Evidence

```
Date:        2026-04-08
Workstream:  Phase 1.5 §5.1.3 Canonical Documentation Alignment
Steps:       A (inventory)
             → B Set 1 (load-bearing probes)
             → C Set 1 (5 patches)
             → B Set 2 (remaining probes)
             → C Set 2 (4 patches)

Build verification:
  $ dotnet build src/platform/host/Whycespace.Host.csproj -nologo -clp:NoSummary -v:m
  → Build succeeded. 0 Warning(s). 0 Error(s).
  → All 8 projects built transitively (Domain, Shared, Projections,
    Systems, Engines, Runtime, Api, Host).

Mechanical guard check:
  $ bash scripts/dependency-check.sh
  → Violations: 0
  → Status: PASS
  → Strengthened predicate (using + fully-qualified + alias) active
    from §5.1.2 Step C-G; the §5.1.3 documentation now accurately
    describes what it enforces.

Folklore sweep verification:
  $ grep -RIn "Phase B[0-9]" src/ --include="*.cs"
  → 0 matches (excluding bin/obj).

  $ grep -RIn "TODO\|FIXME" src/ --include="*.cs"
  → 0 matches.

CLAUDE.md alignment verification:
  - $1a guard discovery directive matches `ls claude/guards/**`.
  - $1b audit discovery directive matches `ls claude/audits/*.audit.md`.
  - `claude/` directory description matches `ls claude/`.
  - $2 canonical prompt store named consistently (`project-prompts/`).

dependency-graph guard/audit alignment verification:
  - R5 body cross-references DG-R5-EXCEPT-01.
  - CODE-LEVEL CHECKS name `scripts/dependency-check.sh` as
    authoritative and split `platform/api` from `platform/host`.
  - LOCK CONDITIONS reference R1–R7 plus DG-* additions.
  - dependency-graph.audit.md PURPOSE names the full rule set;
    C2 predicate matches the script.

Removed artifacts (Step C Set 2):
  - claude/audits.zip → DELETED (option D08-A).
  - claude/project-prompts/phase2/ → DELETED (option D09-A; empty).

Acceptance criteria (§5.1.3 opening pack §2.9 / §4):
  1. Every in-scope documentation artifact enumerated and intent-classified.   ✅
  2. Every CURRENT TRUTH artifact has at least one reproducible probe.         ✅
  3. Every probe has reproducible raw evidence.                                ✅
  4. Every probe result classified ALIGNED / STALE / CONTRADICTED /
     MIS-CATEGORIZED with reason.                                              ✅
  5. Every non-ALIGNED finding has S0–S3 severity.                             ✅
  6. Every non-ALIGNED finding has a remediation patch list entry.             ✅
  7. Folklore-comment sweep covers every src/**/*.cs matching the
     search strings; every citation cross-checked.                             ✅
  8. Discipline-boundary intermixing flagged with a structural fix.            ✅
  9. (No-remediation-during-audit constraint applies to Steps A and B;
     Steps C Set 1 and Set 2 executed under explicit promotion.)               ✅
  10. Newly discovered governance findings captured (DOC-D08/D09/D10
      recorded in this audit; DOC-D07 in Step C Set 2 record).                 ✅
  11. Final report returns explicit terminal status: PASS.                     ✅
  12. README §6.0 row 5.1.3 promoted by this closure pass.                     ✅
```

---

## Final Status Recommendation

**Phase 1.5 §5.1.3 Canonical Documentation Alignment — PASS (2026-04-08).**

All ten drift items closed. CURRENT TRUTH / HISTORICAL BASELINE /
ARCHIVAL RECORD discipline model established and applied. CLAUDE.md
inventories now self-discover from the actual `claude/` tree.
`runtime.guard.md §Dependency Graph & Layer Boundaries` and `dependency-graph.audit.md` aligned
with `scripts/dependency-check.sh` enforcement reality. Stale
`Phase B2a/B2b` source comments cleaned. New-rules promotion-status
integrity restored for the three ambiguous captures. Stale and empty
artifacts removed from the canonical surface. Two non-blocking S3
review items explicitly deferred. Build clean, dependency check clean,
folklore sweep clean. No rule weakened. Ready for promotion to PASS
in README §5.1.3 and §6.0.
