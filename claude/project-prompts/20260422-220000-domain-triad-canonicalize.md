## TITLE

Canonicalize Triad Domain Classifications

## CONTEXT

Classification: domain / multi-system
Context: control-system, core-system, platform-system
Phase: 2.8.25 post-implementation

The repo already contains untracked folder structures for three new domain classifications:
- `src/domain/control-system/` — 90+ .cs files (WIP, compile errors present)
- `src/domain/core-system/` — 21 .cs files (existing but definition too broad)
- `src/domain/platform-system/` — 0 .cs files, folder structure only

These three form the **Triad Model**: core (language/primitives), platform (communication contracts), control (governance/authority).

## OBJECTIVE

Register all three as canonical domain systems in governance documents. Update `domain.guard.md` and related artifacts to formally recognize the triad dependency graph and classification content rules.

## CONSTRAINTS

- Do NOT redesign domain logic or introduce abstractions
- Do NOT create, rename, or delete classification folders (they already exist)
- Fix only structural misalignment with templates 01–06
- Preserve all existing canonical rules; add only new triad-specific rules

## EXECUTION STEPS

1. Update `claude/guards/domain.guard.md`:
   - Change "11 root systems" to "12 root systems" (adding control-system and platform-system)
   - Update core-system definition to: temporal, ordering, identifier primitives only
   - Add control-system entry: system governance and administration
   - Add platform-system entry: communication and messaging contracts
   - Add Triad Dependency Rules section (core→nothing, platform→core only, control→core+platform)
   - Update DOMAIN CLASSIFICATION REGISTRY to include Control (`control-system` / `ControlSystem`) and Platform (`platform-system` / `PlatformSystem`)
   - Update check procedure count references (11→12)

2. Store prompt at `claude/project-prompts/20260422-220000-domain-triad-canonicalize.md`

## OUTPUT FORMAT

- Files modified list
- Canonical system count before/after
- Triad rules registered

## VALIDATION CRITERIA

- `domain.guard.md` lists exactly 12 canonical systems
- Triad dependency rules are explicitly locked (S1 violations)
- Content constraints for each triad classification are enforceable
- No existing canonical rules regressed
