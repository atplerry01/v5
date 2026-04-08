# Phase 1.5 §5.1.3 — Step B Set 1: Load-Bearing Current-Truth Validation

## TITLE
Phase 1.5 §5.1.3 Canonical Documentation Alignment — Step B probe
matrix Set 1 (CT-INV + GG-DG + GG-AUD).

## CLASSIFICATION
system / governance / documentation-alignment

## CONTEXT
Continuation of:
- [§5.1.3 opening pack](20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md)
- [§5.1.3 Step A inventory](20260408-210000-phase-1-5-5-1-3-step-a-canonical-documentation-inventory.md)

This pass executes the seven highest-leverage probes from the Step A
recommended matrix: **CT-INV (B-1..B-3)** validating the CLAUDE.md
inventories against the actual `claude/` tree, **GG-DG (B-4..B-5)**
validating `dependency-graph.guard.md` for internal self-consistency,
and **GG-AUD (B-6..B-7)** validating `dependency-graph.audit.md`
predicate wording against the script reality. No file modifications.

---

## 1. EXECUTIVE SUMMARY

All seven Set 1 probes returned **DRIFT** or **CONTRADICTION**. The
five Step A drift candidates DOC-D01..DOC-D05 are confirmed; one
(DOC-D06) is also confirmed. Hard counts are now in evidence.

**The CLAUDE.md guard list is more stale than Step A estimated.** The
actual top-level guard inventory is **25** files (not 26 as the
inventory table approximated), plus **5** more under
`claude/guards/domain-aligned/` — **30 guard files total**. CLAUDE.md
lists **12**. The omission rate is 18 of 30 = **60%**, including
every `.guard.md` file produced by the §5.1.x hardening series.

**The CLAUDE.md audit list is also worse than estimated.** The actual
top-level audit inventory is **21** active `.audit.md` definitions
(plus 14 `*.audit.output.md` frozen outputs and the
`phase1-evidence/` and `sweeps/` subdirectories). CLAUDE.md lists
**11**. Omission rate: 10 of 21 = **48%**, including
`boundary-purity.audit.md` (2026-04-08) and `dependency-graph.audit.md`.

**`dependency-graph.guard.md` contradicts itself in three places**
(R5 body vs. DG-R5-EXCEPT-01; CODE-LEVEL CHECKS vs. DG-R5-EXCEPT-01;
LOCK CONDITIONS R1–R7 vs. the expanded rule set). The script
(`scripts/dependency-check.sh`) implements the *correct* DG-R5-EXCEPT-01
behaviour; the guard's enforcement-spec section does not match its own
exceptions section. **`dependency-graph.audit.md` C2** has the same
drift as the guard's CODE-LEVEL CHECKS.

No probe was BLOCKED. No Step A drift item was downgraded. One
finding (DOC-D04) is upgraded from S2 to S2-with-three-sites because
LOCK CONDITIONS adds a third self-supersession in the same file.

Recommended advance state: **§5.1.3 IN PROGRESS**, ready for Step C
patch list with a clear ordering by load-bearing impact.

---

## 2. PROBE RESULTS TABLE

| Probe ID | Target Artifact | Claim Tested | Probe Performed | Result | Evidence | Notes |
|---|---|---|---|---|---|---|
| **B-1** | [CLAUDE.md](CLAUDE.md) lines 9–13 | "These 12 guard files are the canonical guard inventory." | `ls claude/guards/*.guard.md \| wc -l` and `ls claude/guards/domain-aligned/*.guard.md` | **CONTRADICTION** | Top-level: 25 files. `domain-aligned/` subtree: 5 files. Total: 30. CLAUDE.md lists 12 (`behavioral, domain, engine, kafka, policy-binding, policy, projection, prompt-container, runtime, structural, systems, tests`). Omitted: `clean-code, composition-loader, config-safety, dependency-graph, determinism, deterministic-id, e2e-validation, hash-determinism, platform, program-composition, replay-determinism, runtime-order, stub-detection` (13 top-level) and the entire `domain-aligned/` subtree (5). 18 of 30 omitted. | Confirms DOC-D01 with hard count. Step A estimated "14+ omitted"; actual is 18. Severity stays S2 (load-bearing per $1a "read ALL guard files"). |
| **B-2** | [CLAUDE.md](CLAUDE.md) lines 18–22 | "These 11 audit files are the canonical audit inventory." | `ls claude/audits/*.audit.md \| wc -l` | **CONTRADICTION** | Top-level: 21 active `*.audit.md` files plus 14 `*.audit.output.md` frozen outputs and `phase1-evidence/` and `sweeps/` subdirectories. CLAUDE.md lists 11 (`activation, behavioral, domain, engine, infrastructure, kafka, policy, projection, runtime, structural, systems`). Omitted: `boundary-purity, clean-code, dependency-graph, determinism, deterministic-id, e2e-validation, platform, replay-determinism, runtime-order, tests` (10 of 21). | Confirms DOC-D02 with hard count. Critical omissions are `boundary-purity.audit.md` (2026-04-08, the §5.1.2 closing artifact) and `dependency-graph.audit.md` (the §5.1.1 closing artifact). Severity stays S2. |
| **B-3** | [CLAUDE.md](CLAUDE.md) lines 80–84 | "Claude orchestration under `claude/`: `guards/`, `audits/`, `new-rules/`, `registry/`, `prompts/`, `agents/`, `templates/`, `workflows/`." Plus internal claim that prompts live in `prompts/`. | `ls claude/` | **CONTRADICTION** (two-axis) | Actual `claude/` root: `agents`, `audits`, `audits.zip`, `guards`, `new-rules`, `project-prompts`, `project-topics`, `prompts`, `registry`, `templates`, `traceability`, `workflows`. (a) Omitted from CLAUDE.md description: `project-prompts`, `project-topics`, `traceability`, `audits.zip`. (b) Internal contradiction with [CLAUDE.md](CLAUDE.md) $2 (line 28): *"`/claude/project-prompts/{YYYYMMDD-HHMMSS}-{classification}-{topic}.md`. Never skip, never overwrite, never store elsewhere."* The directory description says `prompts/` is the prompt store; $2 says `project-prompts/` is the canonical store and forbids storing elsewhere. Both directories actually exist. | Confirms DOC-D03. Adds the `audits.zip` finding (a zip artifact sitting in the live directory; classification UNCLEAR — possibly archival, possibly stale export). Severity raised consideration: stays S3 because the contradicting directives are both visible to a careful reader, but the discipline boundary needs explicit fix. |
| **B-4** | `claude/guards/dependency-graph.guard.md` lines 59–64 (R5 body) | "Allowed references: systems only … None [exceptions] currently granted." | Targeted re-read of lines 59–64 against DG-R5-EXCEPT-01 (lines 164–213) in the same file. | **CONTRADICTION** | DG-R5-EXCEPT-01 explicitly grants the exception and self-documents the contradiction at lines 186–190: *"The prior R5 wording ('Allowed: systems only') was inconsistent with G-PLATFORM-07 and produced a perpetually-tracked violation that was not actually a violation under the canonical platform guard."* The R5 body was never rewritten when the exception was added. A reader landing on R5 first will read a rule that the same file later contradicts. | Confirms DOC-D04. The contradiction is **acknowledged in writing** by DG-R5-EXCEPT-01 but the body lines 59–64 remain unchanged. Severity stays S2. |
| **B-5** | `claude/guards/dependency-graph.guard.md` lines 100–107 (CODE-LEVEL CHECKS) | "Run on every execution: `grep -r 'using .*Runtime' src/platform`" implies any host using of runtime is a violation. | Targeted re-read of lines 100–107 against DG-R5-EXCEPT-01 and against `scripts/dependency-check.sh`. | **CONTRADICTION** | The CODE-LEVEL CHECKS list contains `grep -r "using .*Runtime" src/platform` — this would fail on every legitimate composition-root using under `src/platform/host/composition/runtime/RuntimeComposition.cs`, etc., that DG-R5-EXCEPT-01 explicitly permits. The actual script `scripts/dependency-check.sh` correctly distinguishes `platform/api` (forbidden from runtime/engines/domain/projections) from `platform/host` (permitted under DG-R5-EXCEPT-01) — see [scripts/dependency-check.sh](scripts/dependency-check.sh) lines 84–95. The guard's enforcement spec is self-superseded. | Confirms DOC-D05. The script is the source of truth; the guard text drifted. Severity stays S2. |
| **B-5b** | `claude/guards/dependency-graph.guard.md` lines 147–154 (LOCK CONDITIONS) | "Guard is LOCKED only if: 1. All rules R1–R7 pass" | Cross-reference against the actual rule inventory in the same file (R1–R7 plus DG-R5-EXCEPT-01, DG-R5-HOST-DOMAIN-FORBIDDEN, DG-R7-01, DG-BASELINE-01). | **DRIFT** | The R-numbered rule set has expanded since LOCK CONDITIONS was last edited. R1–R7 is incomplete coverage; the lock criterion should also reference the post-2026-04-07 additions (or at minimum cite "all rules R1–R7 plus DG-* exceptions"). | New finding folded into DOC-D04 as a third drift site in the same file. Severity stays S2. |
| **B-6** | [claude/audits/dependency-graph.audit.md](claude/audits/dependency-graph.audit.md) line 35 (C2 predicate) | "src/platform → must NOT contain Runtime\|Engines\|Domain\|Projections" | Cross-reference against `scripts/dependency-check.sh` and against DG-R5-EXCEPT-01. | **DRIFT** | The audit predicate names `src/platform` as a single forbidden zone; the script correctly splits the predicate into two paths (`scan_using "$SRC/platform/host" "Whycespace\.Domain"` only — runtime is permitted; `scan_using "$SRC/platform/api" "Whyce\.Runtime\|Whyce\.Engines\|Whycespace\.Domain\|Whyce\.Projections"` — runtime forbidden in api). The audit should match the script. | Confirms DOC-D06. Same drift class as DOC-D05; lower leverage because the script overrides at enforcement time. Severity stays S2. |
| **B-7** | `claude/audits/dependency-graph.audit.md` line 11 (INPUTS) | "R1–R7 in `dependency-graph.guard.md`" | Cross-reference against the same expanded rule set in DOC-D04/B-5b. | **DRIFT** | INPUTS lists `R1–R7` only; the rule set has expanded to include `DG-R5-EXCEPT-01`, `DG-R5-HOST-DOMAIN-FORBIDDEN`, `DG-R7-01`, `DG-BASELINE-01`. | Cosmetic; resolves transitively when DOC-D04 is fixed. Severity stays S3. |

---

## 3. CONFIRMED DRIFT LIST

| ID | Title | Severity | Status After Set 1 | Sites |
|---|---|---|---|---|
| **DOC-D01** | CLAUDE.md guard list omits 18 of 30 actual guards | **S2** | **CONFIRMED CONTRADICTION** | [CLAUDE.md](CLAUDE.md) lines 9–13. Hard count: 25 top-level + 5 `domain-aligned/` = 30 actual; 12 listed. |
| **DOC-D02** | CLAUDE.md audit list omits 10 of 21 actual audits | **S2** | **CONFIRMED CONTRADICTION** | [CLAUDE.md](CLAUDE.md) lines 18–22. Hard count: 21 actual; 11 listed. Critical omissions: `boundary-purity.audit.md`, `dependency-graph.audit.md`. |
| **DOC-D03** | CLAUDE.md `claude/` directory description stale + internally inconsistent with $2 | **S3** | **CONFIRMED CONTRADICTION** (two-axis) | [CLAUDE.md](CLAUDE.md) lines 80–84. (a) Omits `project-prompts/`, `project-topics/`, `traceability/`, `audits.zip`. (b) Calls prompt store `prompts/` while $2 prescribes `project-prompts/`. |
| **DOC-D04** | `dependency-graph.guard.md` self-supersedes itself in three places (R5 body, CODE-LEVEL CHECKS, LOCK CONDITIONS) | **S2** | **CONFIRMED CONTRADICTION**, scope expanded from 1 site → 3 sites | Lines 59–64 (R5 body); lines 100–107 (CODE-LEVEL CHECKS); lines 147–154 (LOCK CONDITIONS R1–R7 framing). Self-acknowledged at lines 186–190. |
| **DOC-D05** | `dependency-graph.guard.md` CODE-LEVEL CHECKS predicate forbids legitimate composition-root usings | **S2** | **CONFIRMED CONTRADICTION** | Lines 100–107. Folded into DOC-D04 as a third drift site; the script is the source of truth. |
| **DOC-D06** | `dependency-graph.audit.md` C2 predicate has the same drift as DOC-D05 | **S2** | **CONFIRMED DRIFT** | [claude/audits/dependency-graph.audit.md](claude/audits/dependency-graph.audit.md) line 35; INPUTS line 11 has the same root staleness (R1–R7 framing). |

(DOC-D07 — stale `Phase B*` markers in source comments — was not probed in Set 1; remains pending Step B Set 2.)

No drift was downgraded. No drift was reclassified as ALIGNED. Hard counts now accompany each contradiction.

---

## 4. IMMEDIATE PATCH CANDIDATES

Smallest factual edits that close the confirmed drift. **Not applied in this pass.**

### Patch P-D01 — CLAUDE.md guard list
Replace [CLAUDE.md](CLAUDE.md) lines 9–13 with the full 30-file inventory:
- Top-level (25): `behavioral, clean-code, composition-loader, config-safety, dependency-graph, determinism, deterministic-id, domain, e2e-validation, engine, hash-determinism, kafka, platform, policy-binding, policy, program-composition, projection, prompt-container, replay-determinism, runtime-order, runtime, structural, stub-detection, systems, tests`.
- `domain-aligned/` (5): `economic, governance, identity, observability, workflow`.

Or, more durably, replace the static list with a directive: *"Load every `*.guard.md` file under `claude/guards/**` (including `claude/guards/domain-aligned/**`)."* This survives future additions without requiring CLAUDE.md edits.

### Patch P-D02 — CLAUDE.md audit list
Replace [CLAUDE.md](CLAUDE.md) lines 18–22 with the same directive form: *"Run every `*.audit.md` file under `claude/audits/**`. Frozen `*.audit.output.md` files and the `phase1-evidence/` and `sweeps/` subdirectories are HISTORICAL BASELINE / ARCHIVAL RECORD and are not re-run."*

Optional explicit list (21 files): `activation, behavioral, boundary-purity, clean-code, dependency-graph, determinism, deterministic-id, domain, e2e-validation, engine, infrastructure, kafka, platform, policy, projection, replay-determinism, runtime-order, runtime, structural, systems, tests`.

### Patch P-D03 — CLAUDE.md `claude/` directory description
Replace [CLAUDE.md](CLAUDE.md) lines 80–84 with an actual ls-aligned list and rename the prompt-store entry to match $2:
- `audits/` — post-execution audit definitions and frozen outputs.
- `guards/` — pre-execution guard rules (including `domain-aligned/`).
- `new-rules/` — captured guard/drift findings (including `_archives/`).
- `project-prompts/` — canonical prompt storage per $2.
- `project-topics/`, `traceability/` — supporting tracking artifacts.
- `agents/`, `prompts/`, `registry/`, `templates/`, `workflows/` — supporting orchestration assets.
- `audits.zip` — flag for review (out of canonical scope; classify as HB or remove).

### Patch P-D04 — `dependency-graph.guard.md` R5 body, CODE-LEVEL CHECKS, LOCK CONDITIONS
Three coordinated edits in the same file:
1. **R5 body (lines 59–64):** rewrite to read *"Allowed references: `systems` only, **except where DG-R5-EXCEPT-01 grants composition-root permission to `Whycespace.Host.csproj`**. See DG-R5-EXCEPT-01 below."* Do not delete the rule body; cross-reference the exception so the rule and its exception agree.
2. **CODE-LEVEL CHECKS (lines 100–107):** split the `src/platform` predicate into `src/platform/api` (forbidden: runtime/engines/domain/projections) and `src/platform/host` (forbidden: domain only, per DG-R5-HOST-DOMAIN-FORBIDDEN; runtime/engines/projections permitted under DG-R5-EXCEPT-01). Or, more durably, replace the static grep list with a single line: *"Authoritative enforcement is `scripts/dependency-check.sh`. The patterns below are illustrative."*
3. **LOCK CONDITIONS (lines 147–154):** change "All rules R1–R7 pass" to "All rules R1–R7 and all DG-* exceptions/additions (DG-R5-EXCEPT-01, DG-R5-HOST-DOMAIN-FORBIDDEN, DG-R7-01) pass."

### Patch P-D06 — `dependency-graph.audit.md` C2 + INPUTS
Two coordinated edits in the same file:
1. **C2 (line 35):** split `src/platform` into `src/platform/api` and `src/platform/host` to match the script.
2. **INPUTS (line 11):** change "R1–R7" to reference the full DG-* rule set, or point at `dependency-graph.guard.md` as the authoritative source.

---

## 5. RECOMMENDED STEP C PATCH ORDER

Sequenced by load-bearing impact (highest first). All patches are documentation-only; no source changes.

| Order | Patch | Why First |
|---|---|---|
| 1 | **P-D01** (CLAUDE.md guard list) | Loaded by every prompt per $1a. Fixing this immediately closes the largest drift surface in the repo. The directive form (load `claude/guards/**`) is preferred over a static list because it survives Phase 2 guard additions without further edits. |
| 2 | **P-D02** (CLAUDE.md audit list) | Same load-bearing argument as P-D01, against $1b. `boundary-purity.audit.md` and `dependency-graph.audit.md` should be visible to every future audit sweep. |
| 3 | **P-D04** (`dependency-graph.guard.md` three-site self-supersession) | The most authoritative architecture guard contradicts itself in three places. Fixing this removes the risk that a future agent reads R5/CODE-LEVEL-CHECKS/LOCK-CONDITIONS first and concludes that §5.1.1 closure was a violation. |
| 4 | **P-D06** (`dependency-graph.audit.md` C2 + INPUTS) | Same drift class as P-D04 in the audit definition. Lower priority because the script overrides at enforcement time, but still load-bearing for any human reader. |
| 5 | **P-D03** (CLAUDE.md `claude/` directory description) | Cosmetic relative to P-D01/P-D02, but resolves the internal $2-vs-line-84 contradiction and disposes of the `audits.zip` classification question. |

The five patches are mutually independent and may be applied in a single Step C pass. None of them reopens structural code remediation; none of them modifies a guard rule body's *meaning* — only its *accuracy*. No existing rule is weakened.

---

## 6. STATUS RECOMMENDATION

**§5.1.3 IN PROGRESS.**

- Step A complete (40-row inventory).
- Step B Set 1 complete (this artifact: 7 probes, 6 confirmed drifts, 5 patch candidates, ordered patch list ready).
- Six confirmed S2/S3 contradictions; zero S0/S1 findings; no probe BLOCKED.
- Step B Set 2 (probes B-8..B-15: other guards, new-rules promotion integrity, folklore comment sweep, README §6.0 row-vs-section sweep, `docs/**` classification, `phase2/` directory inspection) remains and may run before or in parallel with Step C — they are independent.
- Step C may begin with the five-patch ordered list above; recommended to execute Step B Set 2 first so any additional drift surfaces in the same Step C pass.
- No source/guard/audit/script/README modifications were made by this Step B Set 1 artifact.

## OUT OF SCOPE
- Any code, guard, audit, script, or README modification.
- Promotion of §5.1.3 status in README §6.0.
- Step B Set 2 probes (B-8..B-15) — deferred to a follow-up artifact.
- Step C remediation — deferred until at least Set 1 patches are queued.
- §5.1.4 and beyond.
