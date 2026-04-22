---
TITLE: E1–Ex Template Alignment — core-system (all contexts)
CLASSIFICATION: core-system
CONTEXT: all (identifier, ordering, temporal)
PHASES_IN_SCOPE: E1 (domain layer structural alignment)
---

# CONTEXT

core-system owns three domain groups (identifier, ordering, temporal) with 11 total leaf domains. All domains are pure value-object primitives with no lifecycle, no state transitions, and no services — per constitutional and domain guard constraints. This prompt aligns all domains against the canonical 7-artifact-folder mandate from domain.guard.md rule 2.

# OBJECTIVE

Ensure every leaf domain under src/domain/core-system/ satisfies the mandatory artifact folder structure (domain.guard.md rule 2 + DS-R3a rule 5). Missing folders must be created with `.gitkeep` placeholders (D0-level).

# CONSTRAINTS

- Anti-Drift ($5): No architecture changes, no new patterns, no renaming, no file moves
- No redesign of domain logic
- Only structural issues and template misalignment are in scope
- core-system rule: NO aggregates with lifecycle, NO state transitions, NO services, NO behavior beyond validation

# EXECUTION STEPS

1. Scan all 11 leaf domains under src/domain/core-system/
2. For each domain, check for the 7 mandatory artifact subfolders
3. Create missing entity/, service/, specification/ .gitkeep files in all 11 domains
4. Verify no regressions

# OUTPUT FORMAT

- List of all 33 .gitkeep files created
- Deviation classification (missing / inconsistent / drift) per finding
- Verification: guard rules satisfied

# VALIDATION CRITERIA

- All 11 domains have all 7 mandatory artifact subfolders
- No existing files modified
- No new domain logic introduced
- domain.guard.md rule 2 + DS-R3a rule 5 satisfied across all 11 domains
