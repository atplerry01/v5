CLASSIFICATION: structural / domain-aligned

SOURCE: Canonical Domain Path Normalisation refactor (operational-system → operational sweep, 2026-04-10)

DESCRIPTION:
The Phase 1 normalisation removed the `-system` suffix from the `operational` and `constitutional` classifications across folder paths, namespaces, infra config, kafka topic templates, and runtime/audit string constants. The remaining `-system` directory trees were NOT renamed in this sweep because they fall outside the immediate todo addressing surface and would expand the blast radius beyond the prescribed refactor boundary.

Residual `-system` folder trees still present:
- src/domain/business-system
- src/domain/constitutional-system  (string constants ARE normalised; folder/namespace not yet)
- src/domain/core-system
- src/domain/decision-system
- src/domain/economic-system
- src/domain/intelligence-system
- src/domain/orchestration-system
- src/domain/structural-system
- src/domain/trust-system
- src/projections/economic-system
- src/projections/governance-system
- src/projections/identity-system
- src/projections/orchestration-system
- src/shared/contracts/events/orchestration-system
- src/shared/contracts/projections/orchestration-system
- tests/integration/constitutional-system
- tests/integration/orchestration-system
- tests/unit/orchestration-system

Note also: the prompt's global rename rule listed `economic-system → economic`, `constitutional-system → constitutional`, `decision-system → decision`. The string-constant `constitutional-system` references have been fixed in this PR (PolicyDecisionEventFactory, PolicyMiddleware, AuditEmission, CommandResult, kafka create-topics.sh, integration tests). Folder/namespace-level renames for those four classifications remain.

PROPOSED_RULE:
Add a structural guard `no-system-suffix.guard.md` under `claude/guards/` that fails the build if any directory under `src/` or `tests/` matches `*-system`, AND any namespace token matching `*System` at the classification position. The guard should be added once the residual trees above are renamed in a follow-up sweep.

Suggested follow-up sweeps (one PR per classification to keep blast radius bounded):
1. constitutional-system → constitutional (folder + namespace)
2. economic-system → economic
3. decision-system → decision
4. orchestration-system → orchestration  (touches projections + shared contracts + tests — largest)
5. business-system, core-system, intelligence-system, structural-system, trust-system, governance-system, identity-system — single sweep (less coupled)

SEVERITY: S2 — STRUCTURAL DRIFT
The residual trees represent inconsistent canonical addressing across classifications. Not breaking, but blocks any future static guard from passing.
