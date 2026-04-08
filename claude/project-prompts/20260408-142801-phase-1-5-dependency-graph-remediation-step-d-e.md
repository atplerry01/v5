# Phase 1.5 §5.1.1 — Dependency Graph Remediation — Steps D + E

## TITLE
Phase 1.5 §5.1.1 Steps D + E — Governance closure and regression prevention
for the dependency graph remediation pass.

## CLASSIFICATION
system / governance / dependency-control

## CONTEXT
Phase 1.5 §5.1.1 Steps A–C reconstructed the actual project reference graph
and removed the only structural violation that remained at csproj level after
the 2026-04-07 remediation cycle:

- Step A — Baseline reconstructed. csproj graph confirmed projections no
  longer references runtime. host carried 7 outbound references; 6 of them
  reclassified as composition-root edges under DG-R5-EXCEPT-01; the 7th
  (host → domain) remained the only true open structural drift.
- Step B — Verification only. `src/projections/**` contains zero `using`
  references into runtime/engines/systems/domain/platform. No code change
  required. DG-R7-01 closure (2026-04-07) re-confirmed.
- Step C — Removed `host → domain`. The sole typed usage was in
  [src/platform/host/adapters/PostgresOutboxAdapter.cs](../../src/platform/host/adapters/PostgresOutboxAdapter.cs)
  inside `ExtractAggregateId`, which pattern-matched on the
  `Whycespace.Domain.SharedKernel.Primitives.Kernel.AggregateId` value
  object. Replaced with a reflection-based `.Value` unwrap; removed the
  `using Whycespace.Domain.SharedKernel.Primitives.Kernel;` line; removed
  the `<ProjectReference Include="..\..\domain\Whycespace.Domain.csproj" />`
  line from
  [src/platform/host/Whycespace.Host.csproj](../../src/platform/host/Whycespace.Host.csproj).
  Compile succeeded; full clean rebuild remains blocked by a running
  `Whycespace.Host` process holding output DLLs locked.

## OBJECTIVE
Execute Steps D (documentation/audit alignment) and E (regression prevention)
to close the governance side of §5.1.1 without re-opening any closed edge
and without scope creep into unrelated cleanup.

## CONSTRAINTS
- No architecture changes beyond what was already executed in Steps A–C.
- No convenience exceptions. Any residual edge must be explicitly justified
  and recorded by name in the guard.
- Preserve canonical WBSM rules and composition-root doctrine
  (CLAUDE.md $5, $7, $15).
- All edits must be minimal, structural, and auditable.
- Do not mark §5.1.1 PASS — full clean build and a green
  `scripts/dependency-check.sh` run remain hold items.

## EXECUTION STEPS

### Step D — Documentation and Audit Alignment

1. **Guard update** —
   [claude/guards/dependency-graph.guard.md](../guards/dependency-graph.guard.md)
   - Narrow `DG-R5-EXCEPT-01` to remove `Whycespace.Domain` from the
     permitted-list for `Whycespace.Host.csproj`.
   - Add new rule **DG-R5-HOST-DOMAIN-FORBIDDEN** (S0): host may not
     reference the domain csproj and may not import `Whycespace.Domain.*`
     in any `*.cs` file under `src/platform/host/**`. Adapters that need
     to inspect domain event shapes must do so via reflection or via
     shared contracts under `src/shared/contracts/**`.
   - Update the ENFORCEMENT MAPPING table row for `platform/host` to
     reflect the DI-only exception list and the explicit domain prohibition.
   - Update the LOCK status block to record that DG-R7-01 was remediated
     2026-04-07 and `host → domain` was remediated 2026-04-08.

2. **Audit update** —
   [claude/audits/dependency-graph.audit.md](../audits/dependency-graph.audit.md)
   - Replace the 2026-04-07 baseline block with a 2026-04-08 baseline that
     records projections=PASS and platform/host=PASS-with-documented-exception.
   - Preserve the prior baseline under a HISTORICAL section with a
     resolution map for traceability.
   - Add `CHECK-DG-HOST-DOMAIN-01` (S0) to the integrated-checks list:
     `scripts/dependency-check.sh` must fail on any reintroduction of
     `host → domain`.

3. **Tracker update** —
   [README.md §5.1.1](../../README.md) (and the 5.1.1 workstream document):
   - Strike the stale "src/projections → runtime" remediation todo and
     replace with a Step B verification-only entry.
   - Replace the "platform/host overreach" todo with a Step C entry that
     records the host → domain removal and reclassifies the remaining
     edges as JUSTIFIED under DG-R5-EXCEPT-01.
   - Move §5.1.1 status from `NOT STARTED` to `IN PROGRESS` and note that
     promotion to PASS requires a clean full-solution build and a green
     `scripts/dependency-check.sh` run.

### Step E — Regression Prevention

4. **Script update** — [scripts/dependency-check.sh](../../scripts/dependency-check.sh)
   - Split the C1 `[platform]` ALLOWED bucket into `[platform_api]` and
     `[platform_host]`. `platform_api` keeps the strict
     `Whycespace.Systems Whycespace.Shared` set; `platform_host` is
     `Whycespace.Api Whycespace.Shared Whycespace.Runtime
     Whycespace.Engines Whycespace.Systems Whycespace.Projections`
     (matches DG-R5-EXCEPT-01; deliberately omits `Whycespace.Domain`).
   - Add a precise belt-and-braces post-C1 check: explicit grep for a
     `<ProjectReference Include="...Whycespace.Domain.csproj" />` line in
     `src/platform/host/Whycespace.Host.csproj`; report
     `DG-R5-HOST-DOMAIN-FORBIDDEN` on hit.
   - Replace the C2 `platform` regex `Whyce\.Domain` (which never matched
     because the domain root namespace is `Whycespace.Domain`, not
     `Whyce.Domain`) with two precise scans:
     - `src/platform/host/**/*.cs` against `Whycespace\.Domain` →
       `DG-R5-HOST-DOMAIN-FORBIDDEN host→domain` (S0).
     - `src/platform/api/**/*.cs` against `Whyce\.Runtime|Whyce\.Engines|
       Whycespace\.Domain|Whyce\.Projections` → `platform/api leakage`.
   - Update the projections C2 scan to also include `Whycespace\.Domain`
     so that any future domain-typed import in projections is caught.
   - Pre-existing script bug fix (in scope because it blocks the green-run
     evidence): exclude `obj/` and `bin/` and limit to `--include='*.cs'`
     in the recursive scans (`scan_using` and the C5 shared-kernel I/O
     scan). Without this, ~50 false positives are produced from NuGet
     `dgspec.json` and `project.assets.json` artifacts.

5. **Guard rule presence** — DG-R5-HOST-DOMAIN-FORBIDDEN is the
   guard-side counterpart to the script check. Both must be present;
   neither alone is sufficient (guard is read fresh on every prompt
   per CLAUDE.md $1a; script is the CI-side enforcement per
   `dependency-graph.guard.md` LOCK condition #4).

6. **Project prompt** — this file
   (`claude/project-prompts/20260408-142801-phase-1-5-dependency-graph-remediation-step-d-e.md`).

## OUTPUT FORMAT
1. Executive Summary
2. Files Changed
3. Guard Changes
4. Audit Changes
5. Script Enforcement Added
6. Tracker / Documentation Changes
7. Verification Notes
8. Remaining Hold Items
9. Final Status: PASS / PARTIAL / FAIL

## VALIDATION CRITERIA
- Guard contains DG-R5-HOST-DOMAIN-FORBIDDEN with S0 severity.
- Guard's DG-R5-EXCEPT-01 no longer lists `Whycespace.Domain`.
- Audit baseline is dated 2026-04-08 and shows projections=PASS,
  platform/host=PASS (with exception).
- README.md §5.1.1 no longer claims `projections → runtime` as an
  active csproj violation.
- `scripts/dependency-check.sh` reports zero violations under
  CHECK-DG-HOST-DOMAIN-01 and zero violations under the C1
  platform_host bucket on the current tree.
- Any remaining script violations (C4 adapter-name over-match against
  `domain/business-system/integration/adapter/**`) are explicitly
  recorded as out-of-scope pre-existing findings.
- §5.1.1 status remains NOT-PASS until full clean build succeeds and
  `scripts/dependency-check.sh` exits 0 (after the unrelated C4
  over-match is addressed in its own workstream).
