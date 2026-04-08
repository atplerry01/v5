# Phase 1.5 §5.1.3 — Step A: Canonical Documentation Inventory

## TITLE
Phase 1.5 §5.1.3 Canonical Documentation Alignment — Step A inventory
and initial drift candidates.

## CLASSIFICATION
system / governance / documentation-alignment

## CONTEXT
Continuation of the §5.1.3 workstream opening pack at
[20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md](20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md).
§5.1.1 PASS (2026-04-08) and §5.1.2 PASS (2026-04-08) are both closed.
This Step A pass is inventory and intent-classification only — no
source/guard/audit/script/README modifications.

---

## 1. EXECUTIVE SUMMARY

Step A enumerated the documentation surface and intent-classified
each artifact (whole-file or section-level) as **CURRENT TRUTH**,
**HISTORICAL BASELINE**, **ARCHIVAL RECORD**, or **UNCLEAR**. The
load-bearing CURRENT TRUTH artifacts are README.md (workstream and
master-tracking sections), CLAUDE.md (execution rules + file
inventories), the 26 files under `claude/guards/**`, the 17 active
audit definitions under `claude/audits/**`, and a small set of
in-source comments that cite canonical rules.

Five immediate drift candidates were surfaced **without running the
full Step B probe matrix**:

1. **CLAUDE.md guard file list (lines 9–13) is severely stale.** It
   names 12 guard files; the actual directory contains **26**
   `.guard.md` files plus a `domain-aligned/` subtree. Critically,
   the omitted files include `dependency-graph.guard.md`,
   `runtime.guard.md`-related neighbors, and the
   recently-strengthened `stub-detection.guard.md` and
   `config-safety.guard.md`. Because $1a mandates loading **every**
   guard file fresh per execution, this list is load-bearing for
   future agents and is a CONTRADICTORY artifact.
2. **CLAUDE.md audit file list (lines 18–22) is severely stale.** It
   names 11 audit files; the actual directory contains 35+ items
   including the just-created `boundary-purity.audit.md` and
   `dependency-graph.audit.md`. Both omissions touch live §5.1.x
   workstream evidence. CONTRADICTORY.
3. **CLAUDE.md `claude/` directory description (lines 80–84) is
   internally contradictory.** It calls the prompt-storage directory
   `prompts/`, but $2 of the same file says canonical storage is
   `/claude/project-prompts/`. The actual repo has both `prompts/`
   *and* `project-prompts/`; canon says only the latter is
   prescribed. STALE + internally CONTRADICTORY.
4. **`dependency-graph.guard.md` R5 rule body (lines 59–64) is
   self-superseded but not rewritten.** The body says
   *“Allowed: systems only … None [exceptions] are currently
   granted.”* The same file's DG-R5-EXCEPT-01 section (lines 164+)
   explicitly grants the exception and self-documents that the prior
   R5 wording was “inconsistent with G-PLATFORM-07.” The R5 body
   was never rewritten when the exception was added; a future reader
   landing on R5 will believe a rule that the same file later
   contradicts. CONTRADICTORY.
5. **`dependency-graph.audit.md` C2 predicate (line 35) names
   `src/platform` as forbidden from referencing
   `Runtime|Engines|Domain|Projections`** — but per
   `DG-R5-EXCEPT-01`, only `src/platform/api` is so restricted;
   `src/platform/host` is the composition root and may reference all
   four (less domain). The audit definition contradicts both the
   guard and the script that enforces it. STALE.

These five candidates are **identified, not yet remediated**. Step B
will run the full probe matrix and may surface additional findings;
Step C will produce the remediation patch list. Recommended advance
state: **IN PROGRESS** with five seed drift items and a tightly
scoped Step B sweep.

---

## 2. CANONICAL DOCUMENTATION INVENTORY TABLE

Discipline categories used throughout:
- **CT** = CURRENT TRUTH (load-bearing for present decisions; must be accurate)
- **HB** = HISTORICAL BASELINE (frozen evidence of a past state; must be marked)
- **AR** = ARCHIVAL RECORD (closed prompt / captured rule / completed audit; preserved verbatim)
- **UC** = UNCLEAR (intent not visible from the artifact itself)

| # | Path | Section / Artifact | Type | Claimed Category | Why It Belongs There | Reproducible Probe Behind CT Claims? | Status | Notes |
|---|------|--------------------|------|------------------|----------------------|-----|--------|-------|
| 1 | [README.md](README.md) | §1.0–§4.0 (Purpose, Objective, Completion Rule, Tracking Model) | doctrine | CT | Stable phase-1.5 doctrine; cited by every workstream prompt. | No probe needed (definitional). | ALIGNED | Stable; no claims that depend on current repo state. |
| 2 | [README.md §5.1.1](README.md) | Dependency Graph Remediation | workstream record | CT | Status `PASS (2026-04-08)`, closure note, and final evidence are all stated as current. | Yes — `bash scripts/dependency-check.sh` and `dotnet build`. | ALIGNED | Verified at §5.1.1 closure and re-verified at §5.1.2 Step C-G. |
| 3 | [README.md §5.1.2](README.md) | Boundary Purity Validation | workstream record | CT | Status `PASS (2026-04-08)`, closure note, final evidence. | Yes — `bash scripts/dependency-check.sh` (strengthened predicate), `dotnet build`, `grep Whycespace.Domain. src/platform/host/`. | ALIGNED | Promoted at §5.1.2 closure. |
| 4 | [README.md §5.1.3](README.md) | Canonical Documentation Alignment | workstream record | CT | Status `NOT STARTED` — opening pack discipline preserved. | n/a (no current-truth claim yet). | ALIGNED | This Step A pass intentionally does not promote it. |
| 5 | [README.md §5.1.4–§5.8.2](README.md) | All other Phase 1.5 workstream sections | workstream record | CT | Status `NOT STARTED` for every row. | n/a (no current-truth claim beyond status). | ALIGNED | Status agrees with §6.0 master tracking table. |
| 6 | [README.md §6.0](README.md) | Master Tracking Table | tracking | CT | Single-source-of-truth for Phase-1.5 row state. | Cross-reference probe against §5.1.x section bodies. | NEEDS REVIEW | Probe required to confirm every row matches its section body; only 5.1.1, 5.1.2, 5.1.3 spot-checked at Step A. |
| 7 | [README.md §7.0–§9.0](README.md) | Governance Decision, Documentation Rule, Executive Summary | doctrine | CT | Stable governance doctrine. | n/a. | ALIGNED | No state-dependent claims. |
| 8 | [CLAUDE.md](CLAUDE.md) §1–$16 | Execution rules | doctrine | CT | Loaded into every prompt context; load-bearing. | n/a (definitional). | ALIGNED | No state-dependent claims. |
| 9 | [CLAUDE.md](CLAUDE.md) lines 9–13 | Guard files list | inventory | CT (claimed) | Reads as a definitive enumeration. | **Probe: `ls claude/guards/*.guard.md`** → 26 files vs. 12 listed. | **CONTRADICTORY** | DRIFT CANDIDATE — see DOC-D01. |
| 10 | [CLAUDE.md](CLAUDE.md) lines 18–22 | Audit files list | inventory | CT (claimed) | Reads as a definitive enumeration. | **Probe: `ls claude/audits/*.audit.md`** → 17 active definitions + 18 outputs vs. 11 listed. | **CONTRADICTORY** | DRIFT CANDIDATE — see DOC-D02. |
| 11 | [CLAUDE.md](CLAUDE.md) lines 80–84 | `claude/` directory description | inventory | CT (claimed) | Reads as a structural overview. | **Probe: `ls claude/`** → root contains `agents/`, `audits/`, `audits.zip`, `guards/`, `new-rules/`, `project-prompts/`, `project-topics/`, `prompts/`, `registry/`, `templates/`, `traceability/`, `workflows/`. | **CONTRADICTORY** + **internally inconsistent** with $2 (canonical prompt path is `/claude/project-prompts/`, not `/claude/prompts/`). | DRIFT CANDIDATE — see DOC-D03. |
| 12 | `claude/guards/dependency-graph.guard.md` | R1–R7 (lines 37–76) | rule body | CT | Numbered canonical rules. | Cross-check vs. EXCEPTIONS section + script + audit. | **CONTRADICTORY** at R5 (lines 59–64). | DRIFT CANDIDATE — see DOC-D04. |
| 13 | `claude/guards/dependency-graph.guard.md` | DG-R5-EXCEPT-01 (lines 164–213) | exception | CT | Explicitly granted exception; load-bearing for §5.1.1 closure. | Verified by §5.1.1 PASS + §5.1.2 Step C-G. | ALIGNED | Self-documents that R5 body wording is stale; the body itself was never updated. |
| 14 | `claude/guards/dependency-graph.guard.md` | DG-R5-HOST-DOMAIN-FORBIDDEN (lines 217–248) | rule body | CT | Newly strengthened in §5.1.2 Step C-G. | Verified by `dependency-check.sh`. | ALIGNED | Five clauses; predicate enumerated. |
| 15 | `claude/guards/dependency-graph.guard.md` | LOCK CONDITIONS (lines 147–154) | meta | CT (claimed) | Lists `R1–R7` as the lock criterion. | The R-numbering set has expanded with DG-R5-EXCEPT-01, DG-R5-HOST-DOMAIN-FORBIDDEN, DG-R7-01, DG-BASELINE-01. | STALE | Minor; lock criterion is incomplete but not actively wrong. |
| 16 | `claude/guards/dependency-graph.guard.md` | CODE-LEVEL CHECKS (lines 100–107) | enforcement spec | CT | Listed as “Run on every execution.” | Includes `grep -r "using .*Runtime" src/platform` — too broad; contradicts DG-R5-EXCEPT-01. | **CONTRADICTORY** | DRIFT CANDIDATE — see DOC-D05. |
| 17 | `claude/guards/dependency-graph.guard.md` | NEW RULES INTEGRATED (lines 157–160, 250–258) | integration log | HB / AR | Dated 2026-04-07; references closed violations DG-R7-01, DG-R5-01, DG-BASELINE-01. | n/a. | NEEDS REVIEW | Mis-categorized: presented inline with current rules but content is historical. Discipline-boundary fix required (banner / `_archives/` relocation). |
| 18 | `claude/guards/runtime.guard.md` | R-DOM-01 body (lines 38–80, post Step C-G) | rule body | CT | Strengthened in §5.1.2 Step C-G; three forbidden forms enumerated; EXEMPT PATHS list updated. | `grep -RIn "Whycespace\.Domain\." src/runtime/` → only the documented exempt paths and the SharedKernel.Primitives carve-out. | ALIGNED | Verified at §5.1.2 closure. |
| 19 | `claude/guards/*.guard.md` (24 other files) | Whole-file | rule body | CT | All present in `claude/guards/`. | **No probe yet** beyond presence. | NEEDS REVIEW | Step B must spot-check each for stale rule bodies, stale STATUS footers, and folklore exemptions of the §5.1.2 Step C-G class. |
| 20 | `claude/guards/domain-aligned/*.guard.md` (5 files) | Whole-file | rule body | CT (presumed) | Living guard files. | **No probe yet.** | NEEDS REVIEW | Subtree omitted from CLAUDE.md guard list. |
| 21 | [claude/audits/dependency-graph.audit.md](claude/audits/dependency-graph.audit.md) | C2 (line 35) | enforcement spec | CT | Listed under CHECKS. | Predicate too broad; contradicts script reality. | **STALE** | DRIFT CANDIDATE — see DOC-D06. |
| 22 | `claude/audits/dependency-graph.audit.md` | INPUTS line 11 | meta | CT | References “R1–R7.” | Same R-numbering staleness as DOC-D04. | STALE (cosmetic) | Cross-references the same drift; resolves with DOC-D04 fix. |
| 23 | [claude/audits/dependency-graph.audit.md](claude/audits/dependency-graph.audit.md) | BASELINE 2026-04-08 block (lines 122–134) | baseline | HB | Explicitly dated; verified by §5.1.1 closure. | n/a (frozen). | ALIGNED | Discipline-boundary OK (clearly dated). |
| 24 | [claude/audits/boundary-purity.audit.md](claude/audits/boundary-purity.audit.md) | Whole-file | audit | AR / CT (terminal status PASS) | Just created at §5.1.2 closure. | Verified during §5.1.2 Step C-G. | ALIGNED | Final audit of §5.1.2; preserved verbatim. |
| 25 | `claude/audits/*.audit.md` (15+ other definitions) | Whole-file | audit definition | CT | Loaded by `$1b`. | **No probe yet.** | NEEDS REVIEW | Step B must spot-check for predicate-vs-implementation drift. |
| 26 | `claude/audits/*.audit.output.md` (10+ files) | Whole-file | frozen audit output | HB / AR | `*.output.md` naming convention implies frozen evidence. | n/a. | ALIGNED (presumptive) | Step B must confirm none are being re-cited as current. |
| 27 | `claude/audits/phase1-evidence/`, `claude/audits/sweeps/` | Subdirectories | frozen evidence | HB | Phase-1 dated evidence. | n/a. | ALIGNED (presumptive) | Confirm by name + date markers. |
| 28 | `claude/audits/phase1-gate.summary.md` | Whole-file | summary | HB / AR | Phase-1 gate summary. | n/a. | NEEDS REVIEW | Confirm not referenced as current in any active prompt. |
| 29 | `claude/project-prompts/20260408-*.md` (top-level §5.1.x prompts, 9 files) | Whole-file | prompt / closure record | AR / CT (terminal claims) | All produced 2026-04-08 during §5.1.x. Closure prompts (5.1.1 PASS closure, 5.1.2 PASS closure) make terminal CT claims about repo state at closure time. | Closure-prompt claims verified at the time of closure. | ALIGNED | Discipline OK (file names are timestamped). |
| 30 | `claude/project-prompts/phase1/*.md` (54 files) | Whole-file | historical prompts | AR | Subdirectory implies archival. | n/a. | ALIGNED | Discipline OK. |
| 31 | `claude/project-prompts/phase2/*.md` | Whole-file | (presumed) future prompts | UC | Phase 2 has not begun; existence is suspicious. | **No probe yet.** | NEEDS REVIEW | Step B must inspect for forward-dated content presented as canonical. |
| 32 | `claude/new-rules/20260408-*.md` (9 active capture files) | Whole-file | new-rule capture | AR / CT (proposed-rule status) | $1c canonical capture format. | Each must declare whether the proposed rule has been promoted into a guard. | NEEDS REVIEW | Step B must verify each capture's promotion status against the actual guard file content. The §5.1.2 Step C-G capture (`20260408-180000-guards.md`) is known ALIGNED (rule promoted in same pass). |
| 33 | `claude/new-rules/_archives/*.md` (28 files) | Whole-file | archived captures | AR | Subdirectory naming. | n/a. | ALIGNED (presumptive) | Discipline OK. |
| 34 | `docs/start-up.md` | Whole-file | operational doc | CT (claimed) | Top-level startup guide. | **No probe yet.** | NEEDS REVIEW | Step B must confirm whether commands and paths still match repo state. |
| 35 | `docs/infrastructure/*.md` (2 files) | Whole-file | operational doc | CT (claimed) | `bootstrap-and-operations-playbook.md`, `quick-reference.md`. | **No probe yet.** | NEEDS REVIEW | Step B must confirm bootstrap commands and config keys still match host implementation post §5.1.x. |
| 36 | `docs/migrations/event-store-ordering.md` | Whole-file | migration note | HB / CT | A migration note may be either historical (already applied) or current (still pending). | **No probe yet.** | NEEDS REVIEW | Step B must classify. |
| 37 | `docs/todo/*.md` (5 files) | Whole-file | working notes | UC | `todo/` naming implies in-progress notes. | n/a. | NEEDS REVIEW | Likely AR, but discipline category not declared in the files themselves. |
| 38 | `docs/validation/e2e-validation-report.md` | Whole-file | report | HB | Report names imply frozen output. | n/a. | NEEDS REVIEW | Confirm report date and that it is not re-cited as current evidence in any active prompt. |
| 39 | `docs/e-series/ex.md` | Whole-file | working note | UC | Sparse subtree. | n/a. | NEEDS REVIEW | Possibly AR; classify in Step B. |
| 40 | `src/**/*.cs` comments citing rule numbers (R-DOM-01, DG-R*, Phase B*) | Comment lines | folklore-or-aligned | CT (cited rules must exist) | Code comments imply correctness. | Cross-check each citation against the cited canonical source. | NEEDS REVIEW | DRIFT CANDIDATE for D11 — see DOC-D07 (stale `Phase B2a/B2b` markers). All R-DOM-01 / DG-R* citations spot-checked appear ALIGNED post §5.1.2 Step C-G. |

---

## 3. INITIAL DRIFT CANDIDATES

| Drift ID | Artifact | Title | Severity | Type | Reason |
|---|---|---|---|---|---|
| DOC-D01 | [CLAUDE.md](CLAUDE.md) lines 9–13 | Guard file list omits 14+ active guards | **S2** | CONTRADICTORY | The CLAUDE.md guard list names 12 files; `claude/guards/` contains 26 `.guard.md` files plus `domain-aligned/`. $1a mandates loading **every** guard file fresh; the canonical inventory is therefore load-bearing and currently lies. Critical omissions include `dependency-graph.guard.md`, `runtime-order.guard.md`, `stub-detection.guard.md`, and `config-safety.guard.md`. |
| DOC-D02 | [CLAUDE.md](CLAUDE.md) lines 18–22 | Audit file list omits 24+ active audits | **S2** | CONTRADICTORY | The CLAUDE.md audit list names 11 files; `claude/audits/` contains 17 active audit definitions plus 18 frozen outputs. The just-created `boundary-purity.audit.md` and the existing `dependency-graph.audit.md` are both omitted, even though they govern the closing §5.1.x workstreams. $1b mandates running the audits; the canonical inventory is load-bearing and currently lies. |
| DOC-D03 | [CLAUDE.md](CLAUDE.md) lines 80–84 | `claude/` directory description stale and internally inconsistent | **S3** | CONTRADICTORY | The description omits `new-rules/`, `project-prompts/`, `project-topics/`, `traceability/`, and `audits.zip`, and calls the prompt-storage directory `prompts/` while $2 of the same file says canonical storage is `/claude/project-prompts/`. Both directories actually exist; canon prescribes only the latter. Internal contradiction with the same file's $2. |
| DOC-D04 | `claude/guards/dependency-graph.guard.md` lines 59–64 | R5 rule body says `Allowed: systems only` and `None [exceptions] currently granted` | **S2** | CONTRADICTORY | Self-superseded by DG-R5-EXCEPT-01 (lines 164+) in the same file, which explicitly grants the exception and self-documents that the prior R5 wording was inconsistent. The body was never rewritten when the exception was added. A future reader landing on R5 first will be misled. |
| DOC-D05 | `claude/guards/dependency-graph.guard.md` lines 100–107 | CODE-LEVEL CHECKS scan `src/platform` as a whole for `using .*Runtime` | **S2** | CONTRADICTORY | The C2-style predicates listed under “Run on every execution” forbid `using .*Runtime` across all of `src/platform`, which would block legitimate composition-root usings that DG-R5-EXCEPT-01 explicitly permits in `src/platform/host/composition/**`. The script (`scripts/dependency-check.sh`) correctly distinguishes `platform/api` from `platform/host`; the guard's enforcement spec does not. |
| DOC-D06 | [claude/audits/dependency-graph.audit.md](claude/audits/dependency-graph.audit.md) line 35 | C2 predicate forbids `src/platform → Runtime|Engines|Domain|Projections` | **S2** | STALE | Same drift class as DOC-D05 — the audit predicate names `src/platform` as a whole instead of distinguishing `platform/api` from `platform/host`. Was correct pre-DG-R5-EXCEPT-01 (2026-04-07); has been stale since 2026-04-07. |
| DOC-D07 | `src/platform/host/composition/IDomainBootstrapModule.cs`, `TodoBootstrap.cs`, `runtime/event-fabric/EventDeserializer.cs`, `EventSchemaRegistry.cs`, plus 9 other `*.cs` files | Stale `Phase B2a/B2b` markers in source comments | **S3** | STALE | 13 in-source comments still reference “Phase B2a”, “Phase B2b” as if they were upcoming or in-progress phases. Phases B2a and B2b are closed; the comments now confuse rather than orient. Per opening pack §2.4 D11 (stale TODO/FIXME/“phase Bxx”-style markers). |

**Confirmed contradictions:** DOC-D01, DOC-D02, DOC-D03, DOC-D04, DOC-D05.
**Stale (non-contradictory) items:** DOC-D06, DOC-D07.
**Review items pending Step B probe matrix:** DOC items 6, 17, 19, 20, 25, 26, 28, 31, 32, 34, 35, 36, 37, 38, 39, 40 in the inventory table.

No S0 findings at Step A depth. No S1 findings. The cluster of S2 contradictions in CLAUDE.md and `dependency-graph.guard.md` is the dominant signal.

---

## 4. HIGH-RISK ARTIFACTS

In descending order of present-decision impact:

1. **CLAUDE.md guard and audit lists (DOC-D01, DOC-D02).** These are the canonical inventory loaded into every prompt context per $1a/$1b. Future agents will rely on them to know what to read. They currently omit dependency-graph, boundary-purity, runtime-order, stub-detection, and config-safety — exactly the artifacts that are doing the most work in the §5.1.x hardening series. Highest leverage of any finding.
2. **`dependency-graph.guard.md` R5 body and CODE-LEVEL CHECKS (DOC-D04, DOC-D05).** The same file simultaneously asserts R5 with no exceptions (top half) and grants DG-R5-EXCEPT-01 (bottom half). The bottom half is correct and the top half is stale. A reader of the top half could incorrectly flag the entire §5.1.1 closure as a violation.
3. **`dependency-graph.audit.md` C2 (DOC-D06).** Same drift class as DOC-D05 but in the audit rather than the guard. Lower leverage because the script overrides it, but still load-bearing for any human reading the audit definition.
4. **`claude/new-rules/**` promotion-status integrity (inventory item 32).** Each capture file should declare whether its proposed rule has been promoted into a guard. Step B must verify; only the §5.1.2 Step C-G capture is known ALIGNED.
5. **README §6.0 row-vs-section consistency (inventory item 6).** Spot-checked at 5.1.1, 5.1.2, 5.1.3 only. Step B should sweep all 27 rows.
6. **In-source folklore comments (inventory item 40).** The §5.1.2 Step C-G discovery is the prototype; Step B must prove no further instances of the same drift class survive elsewhere.

---

## 5. RECOMMENDED STEP B PROBE MATRIX

Narrowest, highest-value sweep. Probes are evidence-first: each is a single command with an expected ALIGNED shape.

**Probe set CT-INV (CLAUDE.md inventory truth)**
- **B-1.** `ls claude/guards/*.guard.md | wc -l` and `ls claude/guards/domain-aligned/*.guard.md | wc -l` — expected: matches the line count of the CLAUDE.md guard list. **Resolves DOC-D01.**
- **B-2.** `ls claude/audits/*.audit.md | wc -l` — expected: matches the CLAUDE.md audit list. **Resolves DOC-D02.**
- **B-3.** `ls claude/` — expected: matches the CLAUDE.md `claude/` overview verbatim. **Resolves DOC-D03.**

**Probe set GG-DG (dependency-graph guard self-consistency)**
- **B-4.** Targeted re-read of `dependency-graph.guard.md` lines 59–64 against lines 164–213 — expected: R5 body either references DG-R5-EXCEPT-01 or is rewritten to match. **Resolves DOC-D04.**
- **B-5.** Targeted re-read of `dependency-graph.guard.md` CODE-LEVEL CHECKS (lines 100–107) — expected: the `src/platform` predicate is split into `platform/api` and `platform/host` with the correct DG-R5-EXCEPT-01 carve-out, **or** a pointer to `scripts/dependency-check.sh` as the authoritative enforcement. **Resolves DOC-D05.**

**Probe set GG-AUD (audit definitions self-consistency)**
- **B-6.** `dependency-graph.audit.md` C2 predicate — expected: split between `platform/api` and `platform/host`. **Resolves DOC-D06.**
- **B-7.** Spot-check 3 other `*.audit.md` files (`runtime.audit.md`, `structural.audit.md`, `engine.audit.md`) for the same C2-style drift class.

**Probe set GG-OTHER (other guards spot-check)**
- **B-8.** Open `runtime.guard.md`, `platform.guard.md`, `structural.guard.md`, `engine.guard.md`, `program-composition.guard.md`, `kafka.guard.md`, `policy.guard.md`, `policy-binding.guard.md`, `projection.guard.md`, `systems.guard.md`, `tests.guard.md` — for each, identify any rule body that has been self-superseded by a NEW RULES INTEGRATED section in the same file (the DOC-D04 pattern).
- **B-9.** Sweep `claude/guards/domain-aligned/**` (5 files) for stated discipline category.

**Probe set NR (new-rules promotion integrity)**
- **B-10.** For each file in `claude/new-rules/20260408-*.md`, grep its PROPOSED_RULE text against the canonical guards to determine if the rule has been promoted. Expected: each file either (a) has a `STATUS: APPLIED` clause naming the destination guard, or (b) is honestly pending.

**Probe set FOL (folklore comment sweep)**
- **B-11.** `grep -RIn "Phase B[0-9]\|TODO\|FIXME" src/ --include="*.cs"` — expected: zero hits, OR every hit explicitly tied to a still-open phase. **Resolves DOC-D07.**
- **B-12.** `grep -RIn "R-DOM-01\|DG-R[0-9]" src/ --include="*.cs"` — for each hit, cross-check the cited rule against its canonical source. Expected: every citation aligned post-§5.1.2 Step C-G.

**Probe set TR (README §6.0 row-vs-section integrity)**
- **B-13.** For each row in the §6.0 master tracking table, grep the section body for the same status string. Expected: every row matches its section.

**Probe set DOCS (docs/** classification)**
- **B-14.** Read the date and discipline-category of each file under `docs/**`; classify each as CT, HB, AR, or UC. No remediation, classification only.

**Probe set PP-PHASE2 (phase2 prompt directory)**
- **B-15.** Inspect `claude/project-prompts/phase2/*.md` — expected: either honestly empty / placeholder, or clearly marked as forward-looking design notes (HB / AR), not as CT.

Out of Step B scope: any remediation patch, any guard or audit edit, any source rewrite. Step B is classification-only.

---

## 6. STATUS RECOMMENDATION FOR §5.1.3

**IN PROGRESS** — Step A executed successfully. Five immediate drift candidates surfaced (three S2 contradictions in CLAUDE.md, two S2 contradictions in `dependency-graph.guard.md`, one S2 staleness in `dependency-graph.audit.md`, and one S3 in-source folklore cluster). Step B probe matrix is scoped (15 probes across six probe sets) and ready to execute. No blockers. No source/guard/audit/script/README modifications were made by this Step A artifact. README §6.0 row 5.1.3 may be advanced from `NOT STARTED` to `IN PROGRESS` in a follow-up tracking-only edit; this artifact intentionally does not promote it, per opening-pack discipline.

---

## OUT OF SCOPE
- Any code, guard, audit, script, or README modification.
- Promotion of §5.1.3 status in README §6.0.
- Any actual rewrite of stale rule bodies (deferred to Step C).
- §5.1.4 and beyond.
- Tooling/automation for documentation linting.
