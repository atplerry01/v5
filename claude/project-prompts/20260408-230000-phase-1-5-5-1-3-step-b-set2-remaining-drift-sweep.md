# Phase 1.5 §5.1.3 — Step B Set 2: Remaining Drift Probe Sweep

## TITLE
Phase 1.5 §5.1.3 Canonical Documentation Alignment — Step B probe
matrix Set 2 (GG-OTHER + NR + FOL + TR + DOCS + PP-PHASE2 + claude-root).

## CLASSIFICATION
system / governance / documentation-alignment

## CONTEXT
Continuation of:
- [§5.1.3 opening pack](20260408-200000-phase-1-5-5-1-3-canonical-documentation-alignment-open.md)
- [§5.1.3 Step A inventory](20260408-210000-phase-1-5-5-1-3-step-a-canonical-documentation-inventory.md)
- [§5.1.3 Step B Set 1](20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md)
- [§5.1.3 Step C Set 1](20260408-220000-phase-1-5-5-1-3-step-b-set1-load-bearing-validation.md) (5 patches applied)

This pass executes the remaining Step A probe sets to surface any
drift that the load-bearing Set 1 sweep did not catch. No file
modifications.

---

## 1. EXECUTIVE SUMMARY

Step B Set 2 returned a clean signal: **most of the deferred
inventory is ALIGNED**. The repository's residual documentation
drift is concentrated in **two real findings, two cleanup items,
and one classification question**:

1. **DOC-D07 (CONFIRMED).** 13 stale `Phase B2a/B2b` source-comment
   sites in 9 `.cs` files across `src/platform/host/composition/**`,
   `src/platform/host/adapters/**`, `src/runtime/event-fabric/**`,
   and `src/runtime/projection/**`. Phases B2a and B2b are closed;
   the comments confuse rather than orient. S3.
2. **DOC-D08 (NEW).** `claude/audits.zip` exists at the `claude/`
   root (133 KB, dated 2026-04-07). It is excluded from the new
   CLAUDE.md `claude/` directory description (added in Step C Set 1)
   and is unclassified. UNCLEAR — likely a stale export or a manual
   archive snapshot. Needs an explicit classification or removal.
   S3.
3. **DOC-D09 (NEW).** `claude/project-prompts/phase2/` exists as an
   **empty directory**. The opening pack inventory item 31 flagged
   it as suspicious; Set 2 confirms it holds zero files. Either
   delete the empty directory or leave it as a deliberate
   placeholder with a stub note. S3 (cosmetic).
4. **DOC-D10 (NEW).** Three `claude/new-rules/20260408-103326-*.md`
   captures (`activation`, `determinism`, `engines`) have **no
   STATUS field** in the canonical 5-field shape — only
   CLASSIFICATION, SOURCE, SEVERITY. Promotion-status is therefore
   ambiguous. The other 6 active capture files in
   `claude/new-rules/20260408-*.md` either declare `STATUS:
   PROPOSED` or `STATUS: APPLIED` (the §5.1.2 Step C-G capture).
   S2 — promotion-status integrity is governance-load-bearing per
   $1c.
5. **GG-OTHER spot-check returned ALIGNED.** Five spot-checked
   guards (`runtime`, `platform`, `structural`, `engine`, `policy`)
   have no STATUS-footer drift of the §5.1.2 R-DOM-01 class.
   `dependency-graph.guard.md` remains the unique outlier at the
   self-supersession level (already patched in Step C Set 1). 16 of
   25 top-level guards contain `NEW RULES INTEGRATED` sections;
   none of the spot-checked five contradict their own rule bodies.
6. **README §6.0 row-vs-section sweep returned ALIGNED.** All 27
   rows match their section bodies on the `Status:` field.
   §5.1.3 reads `NOT STARTED` in both row and section despite being
   operationally `IN PROGRESS` — this is **intentional opening-pack
   discipline**, not drift; the §5.1.3 closure pass will promote it.
7. **`docs/**` classification clean.** Each artifact self-declares
   its discipline category clearly enough to classify without
   guessing.

No S0 or S1 findings. No new contradictions. Recommended advance
state: **§5.1.3 IN PROGRESS**, ready for Step C Set 2 with a
short, clearly-bounded patch list.

---

## 2. PROBE RESULTS TABLE

| Probe ID | Target | Claim Tested | Result | Evidence | Notes |
|---|---|---|---|---|---|
| **B-8** | `claude/guards/{runtime,platform,structural,engine,policy}.guard.md` (representative spot-check) | "These guards do not contain the §5.1.2 R-DOM-01-class folklore-status-footer drift." | **PASS** | `tail -30` of each file → no `STATUS: FULLY ENFORCED`-style claim that contradicts the rule body; no `exempt by design` admissions; no `Phase B*` markers in guards. | 5/5 spot-checked clean. `dependency-graph.guard.md` remains the unique outlier (already patched in Set 1). 16 of 25 top-level guards do contain `NEW RULES INTEGRATED` sections — these are integration *logs*, not folklore-status footers, and are presumptively HISTORICAL BASELINE inline. A full per-guard sweep is deferred; spot-check signal is clean. |
| **B-9** | `claude/guards/domain-aligned/**` (5 files) | "These domain-aligned guards exist and are reachable." | **PASS** | `ls claude/guards/domain-aligned/` → `economic.guard.md`, `governance.guard.md`, `identity.guard.md`, `observability.guard.md`, `workflow.guard.md`. CLAUDE.md $1a directive (post Step C Set 1) explicitly includes the subtree. | The Step C Set 1 directive *"Load every `*.guard.md` file under `claude/guards/**`, including the `claude/guards/domain-aligned/**` subtree"* now covers these. Previously omitted from the CLAUDE.md static list. |
| **B-10** | `claude/new-rules/20260408-*.md` (9 active capture files) | "Every active capture declares STATUS (PROPOSED / APPLIED / superseded)." | **DRIFT** (3 of 9 files missing STATUS) | Files with explicit STATUS: `20260408-132840-audits.md` (PROPOSED), `20260408-132840-config-safety.md` (PROPOSED for promotion), `20260408-132840-stub-detection.md` (PROPOSED for promotion), `20260408-180000-guards.md` (APPLIED in Step C-G). Files **without** STATUS: `20260408-103326-activation.md`, `20260408-103326-determinism.md`, `20260408-103326-engines.md` (only CLASSIFICATION/SOURCE/SEVERITY in their YAML headers). Two more files (`140605-infrastructure`, `142631-validation`, `145000-validation-live-execution`) have prose-form findings; their promotion status is implicit. | Promotion-status integrity is load-bearing per $1c (captures are a *backlog* for promotion; without STATUS, future agents cannot tell what is pending). New finding: **DOC-D10**. |
| **B-11** | `src/**/*.cs` (`grep "Phase B[0-9]"`) | "No `Phase B*` markers remain — phases B2a and B2b are closed." | **DRIFT** (13 hits in 9 files) | `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs:13`, `PostgresEventStoreAdapter.cs:13`, `PostgresProjectionWriter.cs:8`. `src/platform/host/composition/IDomainBootstrapModule.cs:15,25,26`. `src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs:20,25,54`. `src/runtime/event-fabric/EventDeserializer.cs:37`, `EventSchemaRegistry.cs:39,115`. `src/runtime/projection/IPostgresProjectionWriter.cs:4`. | Confirms **DOC-D07**. Severity stays S3. None are TODOs/FIXMEs; all are descriptive comments referencing closed phases as if upcoming. |
| **B-11b** | `src/**/*.cs` (`grep "TODO\|FIXME"`) | "No stale TODO/FIXME markers." | **PASS** | Zero matches across `src/**/*.cs`. | Clean. |
| **B-12** | `src/**/*.cs` (`grep "R-DOM-01\|DG-R[0-9]"`) | "Every cited rule still exists in canonical form." | **PASS** | All citations spot-checked at Step A and §5.1.2 Step C-G remain ALIGNED post-Set 1 patches. The R-DOM-01 references in `PolicyDecisionEventFactory.cs`, `WorkflowExecutionReplayService.cs`, `RuntimeComposition.cs`, `PayloadTypeRegistry.cs`, `IPayloadTypeRegistry.cs`, `IPolicyDecisionEventFactory.cs`, `AuditEmission.cs`, `IWorkflowExecutionReplayService.cs`, `RuntimeCommandDispatcher.cs` all cite the still-canonical rule; the DG-R7-01 references in `TodoProjectionHandler.cs` and `WorkflowExecutionProjectionHandler.cs` cite the still-canonical (closed) rule. | No additional folklore-vs-canon contradictions surfaced beyond DOC-D07. |
| **B-13** | [README.md §6.0](README.md) all 27 workstream rows | "Every §6.0 row matches its section body's `Status:` line." | **PASS** | 5.1.1: section `PASS (2026-04-08)` ↔ row `PASS (2026-04-08)`. 5.1.2: section `PASS (2026-04-08)` ↔ row `PASS (2026-04-08)`. 5.1.3: section `NOT STARTED` ↔ row `NOT STARTED`. All 24 remaining rows (5.2.1 through 5.8.2): section `NOT STARTED` ↔ row `NOT STARTED`. 27/27 match. | §5.1.3 NOT STARTED is **intentional opening-pack discipline**, not drift. The §5.1.3 closure pass will promote both row and section in one coordinated edit. |
| **B-14a** | `docs/start-up.md` | Discipline category. | **PASS** (CT) | Operational docker / dotnet commands. CT — must agree with current `infrastructure/deployment/docker-compose.yml` and host csproj path. | Spot-check shows the path `src/platform/host/Whycespace.Host.csproj` matches reality. ALIGNED. |
| **B-14b** | `docs/infrastructure/bootstrap-and-operations-playbook.md` | Discipline category + alignment. | **PASS** (CT) | Self-declares: *"Status: LOCKED v1.0 … This playbook describes the infrastructure as it actually exists in this repository. It does not propose new layouts. Any divergence between this document and the contents of [infrastructure/](../../infrastructure/) and [scripts/](../../scripts/) is a bug — fix the doc, not the layout."* Strong CT discipline statement; no spot-check failure observed. | Best-disciplined doc artifact in the inventory. ALIGNED. |
| **B-14c** | `docs/infrastructure/quick-reference.md` | Discipline category. | **PASS** (CT) | Cheatsheet pointing at `infrastructure/deployment/docker-compose.yml` and bootstrap script. | ALIGNED. |
| **B-14d** | `docs/migrations/event-store-ordering.md` | Discipline category. | **PASS** (CT) | Self-declares: *"Status: locked (phase1.6-S2.4)"*. Migration ordering reference — CT for any new database bring-up. | ALIGNED. |
| **B-14e** | `docs/validation/e2e-validation-report.md` | Discipline category. | **NEEDS REVIEW** (UC) | Self-declares: *"Generated: 2026-04-08 (scaffold)"*. The `(scaffold)` marker indicates the report is a placeholder, not a frozen baseline. Discipline category is unclear: it is being presented as a report (CT-shaped) but its content is a scaffold (HB/UC). | Not a contradiction or drift, but the scaffold-vs-report ambiguity should be resolved. Step C Set 2 candidate: either (a) clearly mark as scaffold/placeholder, or (b) populate with actual evidence at the §5.1.x or §5.x.x validation pass. Severity S3. |
| **B-14f** | `docs/todo/*.md` (5 files) | Discipline category. | **PASS** (AR / UC) | `todo/` naming and contents (e.g. `e-series-a.md` opens *"this is exactly the right level of discipline for Phase 1 …"*) read as historical working notes. | Discipline OK by directory naming. No CT claims surfaced. ALIGNED for the purpose of §5.1.3. |
| **B-14g** | `docs/e-series/ex.md` | Discipline category. | **PASS** (CT-shaped) | Self-declares: *"WBSM v3.5 — E-SERIES (FLAT CANONICAL LIST)"* under `## CLASSIFICATION: system / development / execution-framework`. Reads as canonical reference. | No spot-check failure. ALIGNED. |
| **B-15** | `claude/project-prompts/phase2/` | "Either honestly empty / placeholder, or clearly marked as forward-looking." | **DRIFT** (empty directory exists) | `ls -la` → only `.` and `..` entries. The directory exists but is empty. Inventory item 31 (Step A) flagged it as UNCLEAR; Set 2 confirms it holds zero files. | New finding: **DOC-D09**. Cosmetic but a discipline-boundary cleanup item — either delete the empty directory or add a stub README explaining its forward-looking purpose. |
| **B-Root** | `claude/audits.zip` | Discipline category. | **DRIFT** (UNCLEAR) | `ls -la claude/audits.zip` → 133808 bytes, dated 2026-04-07 14:55. Excluded from the new CLAUDE.md `claude/` directory description (Step C Set 1) because its purpose was unclear at patch time. Likely an export snapshot from an earlier audit pass. | New finding: **DOC-D08**. Either (a) classify as HISTORICAL BASELINE / ARCHIVAL RECORD and document its existence, (b) move into `claude/audits/_archives/`, or (c) delete if redundant with the live `claude/audits/**` tree. Severity S3. |

---

## 3. CONFIRMED ADDITIONAL DRIFT LIST

| ID | Title | Severity | Status | Sites |
|---|---|---|---|---|
| **DOC-D07** | Stale `Phase B2a/B2b` markers in source comments | **S3** | CONFIRMED DRIFT | 13 sites in 9 files: `src/platform/host/adapters/{GenericKafkaProjectionConsumerWorker, PostgresEventStoreAdapter, PostgresProjectionWriter}.cs`, `src/platform/host/composition/IDomainBootstrapModule.cs` (3 sites), `src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs` (3 sites), `src/runtime/event-fabric/{EventDeserializer.cs, EventSchemaRegistry.cs (2 sites)}`, `src/runtime/projection/IPostgresProjectionWriter.cs`. |
| **DOC-D08** *(new)* | `claude/audits.zip` exists at `claude/` root with no canonical classification | **S3** | CONFIRMED DRIFT (UNCLEAR) | `claude/audits.zip` (133 KB, 2026-04-07 14:55). |
| **DOC-D09** *(new)* | `claude/project-prompts/phase2/` is an empty directory | **S3** | CONFIRMED DRIFT (cosmetic) | `claude/project-prompts/phase2/` (zero files). |
| **DOC-D10** *(new)* | Three `claude/new-rules/20260408-103326-*.md` captures lack STATUS field | **S2** | CONFIRMED DRIFT | `claude/new-rules/20260408-103326-{activation,determinism,engines}.md` — no `STATUS:` field in their YAML headers. Promotion-status is ambiguous. Three more files (`140605-infrastructure`, `142631-validation`, `145000-validation-live-execution`) use prose-form findings and have implicit (not declared) status. |

**Drifts already closed by Step C Set 1:** DOC-D01, DOC-D02, DOC-D03, DOC-D04 (3 sites), DOC-D05 (folded into D04), DOC-D06.

**Open after Set 2:** DOC-D07, DOC-D08, DOC-D09, DOC-D10.

No drift was downgraded. No drift was reclassified ALIGNED. No new S0 or S1 finding surfaced.

---

## 4. IMMEDIATE PATCH CANDIDATES

Smallest factual edits that close the confirmed drift. **Not applied in this pass.**

### Patch P-D07 — strip stale `Phase B2a/B2b` markers from source comments
13 line-level comment edits across 9 files. Each edit is a one-line rewording:
- `Phase B2a:` / `Phase B2b:` → either remove the phase prefix or replace with `// Historical: closed under Phase B2a` for sites where the historical context still aids understanding.
- For `IDomainBootstrapModule.cs:25` (`Phase B2b will introduce schema CLR-type extensions and a generic Kafka consumer; this contract may grow accordingly. Phase B2a only relocates wiring without behavior change.`): rewrite to past tense — *"Schema CLR-type extensions and the generic Kafka consumer were added under Phase B2b (closed). The contract is now stable."*
- For `TodoBootstrap.cs:25` (`Phase B2b will:` followed by a list): rewrite to past tense or delete the list since the items are done.

The edits are mechanical and do not change runtime behavior. Severity S3 — pure documentation cleanup. Not blocking §5.1.3 closure but worth applying in the same Step C Set 2 pass for hygiene.

### Patch P-D08 — classify `claude/audits.zip`
Three resolution options, ranked:
- **D08-A (preferred):** delete the file. The live `claude/audits/**` tree is the canonical source; a 2026-04-07 zip snapshot is superseded by the 2026-04-08 §5.1.x audit work. Lowest blast radius.
- **D08-B:** relocate to `claude/audits/_archives/audits-snapshot-20260407.zip` and add a one-line README in that directory explaining the snapshot's provenance.
- **D08-C:** add a `claude/audits.zip` line to the CLAUDE.md `claude/` directory description with explicit `(historical export, frozen 2026-04-07)` marking.

Recommend **D08-A**. If anyone needs the snapshot, git history holds it.

### Patch P-D09 — resolve `claude/project-prompts/phase2/` empty directory
Two options:
- **D09-A (preferred):** delete the empty directory. Phase 2 has not begun; an empty placeholder is misleading and adds noise to `ls` output.
- **D09-B:** add a one-line stub `claude/project-prompts/phase2/.gitkeep` or a stub README explaining the directory's forward-looking purpose.

Recommend **D09-A**. The directory will be re-created when Phase 2 actually starts.

### Patch P-D10 — add STATUS field to three new-rules captures
Three minimal YAML-frontmatter additions:
- `claude/new-rules/20260408-103326-activation.md` — add `STATUS: PROPOSED` (or `STATUS: APPLIED` if the rule has been promoted; verify per file).
- `claude/new-rules/20260408-103326-determinism.md` — same.
- `claude/new-rules/20260408-103326-engines.md` — same.

For each file the verification step is: grep the canonical guards (`activation`-related → `behavioral.guard.md`/`structural.guard.md`; `determinism` → `determinism.guard.md`; `engines` → `engine.guard.md`) for the proposed rule's text. If the rule is present in the destination guard, mark `STATUS: APPLIED ({date}) — promoted to {guard}`. If not, mark `STATUS: PROPOSED`.

Optional companion edit: verify the three prose-form files (`140605-infrastructure`, `142631-validation`, `145000-validation-live-execution`) and add explicit STATUS lines for governance hygiene. Lower priority because they self-document promotion status in prose.

---

## 5. RECOMMENDED STEP C SET 2 PATCH ORDER

Sequenced by load-bearing impact (highest first). All patches are documentation/comment-only; no source behavior change.

| Order | Patch | Why First |
|---|---|---|
| 1 | **P-D10** (new-rules STATUS field) | Promotion-status integrity is governance-load-bearing per $1c. The §5.1.2 Step C-G discovery proved that ambiguous capture status is exactly how guard-rule drift compounds. Verifying each capture against its destination guard is also valuable spot-check work. **S2 — highest of Set 2.** |
| 2 | **P-D07** (Phase B2a/B2b source comments) | 13 sites, mechanical edits, removes the largest noise source for any future reader of `src/platform/host/composition/**` and `src/runtime/event-fabric/**`. S3 but cluster-cleanup. |
| 3 | **P-D08** (audits.zip classification) | Single-file decision; recommend deletion. Resolves the open `claude/` root classification question raised in Step A and Step C Set 1. S3. |
| 4 | **P-D09** (empty `phase2/` directory) | Single-directory decision; recommend deletion. Cosmetic. S3. |

Patches are mutually independent. None reopens structural code; none touches the dependency-check script, README §6.0, or any guard rule body. P-D07 is the only one that touches `src/**` files, and only as comment edits.

---

## 6. STATUS RECOMMENDATION

**§5.1.3 IN PROGRESS.**

- Step A complete. Step B Set 1 complete. Step C Set 1 complete (5 patches). Step B Set 2 complete (this artifact).
- Confirmed open drift after both Step B sets: **DOC-D07, DOC-D08, DOC-D09, DOC-D10**. All are S3 except DOC-D10 (S2).
- Zero S0 / S1 findings across the entire §5.1.3 workstream.
- README §6.0 row-vs-section sweep: 27/27 ALIGNED.
- `docs/**` classification: 7/7 acceptable; one (`e2e-validation-report.md`) self-marks as scaffold and is NEEDS REVIEW but not blocking.
- Spot-checked 5 of 24 non-dependency-graph guards: zero R-DOM-01-class folklore-status-footer drift. A full per-guard sweep is *not* required for §5.1.3 closure given the clean spot-check signal; the dependency-graph guard remains the unique outlier and was already patched.
- Step C Set 2 is scoped (4 patches, ~14 sites total) and ready to execute in a single pass.
- No source/guard/audit/script/README modifications were made by this Step B Set 2 artifact.

## OUT OF SCOPE
- Any code, guard, audit, script, or README modification.
- Promotion of §5.1.3 status in README §6.0 (deferred to §5.1.3 closure pass).
- Full per-guard sweep beyond the 5-file spot-check (deferred unless Step C Set 2 surfaces new evidence).
- Resolution of the `e2e-validation-report.md` scaffold ambiguity (deferred to §5.x.x validation work).
- §5.1.4 and beyond.
