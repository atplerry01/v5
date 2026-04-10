# TITLE
Canonical Domain Path Normalisation — remove `-system` suffix and enforce classification/context/domain triplet

# CONTEXT
WBSM v3.5 structural refactor. The codebase mixes `operational-system` (folder/namespace) with the canonical `operational` triplet form already used by runtime DomainRoute strings, Kafka topics, and policy IDs. This refactor aligns folder paths and namespaces to the canonical form so addressing is uniform across layers.

# OBJECTIVE
Enforce canonical addressing `classification/context/domain` everywhere. Remove `-system` suffix from `operational`, `economic`, `constitutional`, `decision` classifications across folder paths, namespaces, routing metadata, event names, topic names, and constants.

# CONSTRAINTS
- No business-logic changes
- No domain entity renames (Todo stays Todo)
- No new abstractions
- No event payload modifications
- No moving the three-level nesting scheme
- Only addressing/naming/routing fixes

# EXECUTION STEPS
1. Survey all surfaces (domain, projections, systems, engines, runtime, contracts, tests, infra)
2. Rename `OperationalSystem` namespace → `Operational` in all .cs files
3. `git mv` `operational-system/` folders → `operational/`
4. Fix `constitutional-system` string content references
5. Update infra environment.json classification fields
6. Build, fix any breakages
7. Capture residual `-system` trees (economic, decision, constitutional, plus non-listed) as new-rules backlog
8. Commit with prescribed message

# OUTPUT FORMAT
Direct file edits + git mv operations + commit.

# VALIDATION CRITERIA
- `dotnet build` succeeds
- `grep -rI "operational-system" src/ tests/ infrastructure/` returns empty
- All Todo addressing uses canonical triplet at folder + namespace + string layers
- No runtime/behaviour regressions

# CLASSIFICATION
domain: structural
context: refactor
classification: addressing-normalisation
