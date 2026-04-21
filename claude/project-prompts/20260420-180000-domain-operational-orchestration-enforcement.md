---
classification: domain
context: operational-orchestration
domain: boundary-enforcement
status: inventory-only
---

# Operational-Orchestration Boundary Enforcement Pass (Phase 1 Inventory)

## TITLE
Operational / Orchestration-Layer Enforcement — aggregate purity + handler discipline

## CONTEXT
User prompt uses "operational layer" as a generic label for the application-orchestration surface. In this codebase that maps to:
- `src/engines/**` — command handlers (T0U policy, T1M workflow steps, T2E business logic, T3I integration)
- `src/systems/**` — composition (upstream/midstream/downstream)
- `src/runtime/**` — middleware + event-fabric + control-plane

"Domain truth" = `src/domain/**`. Aggregates purity rules are already partially enforced by `domain.guard.md` and ArchTests from prior passes. This pass inspects the boundary and pins it with new ArchTests.

User instruction: **Start with Phase 1 inventory. Stop and report before Phase 2+ refactoring / Phase 8 enforcement lock.**

## OBJECTIVE (Phase 1 only)
- Enumerate engine handlers by type.
- Scan domain aggregates for cross-aggregate calls, repository calls, temporal logic, multi-step orchestration (red flags).
- Identify process managers / sagas and their home.
- Flag any application services holding mutable state.

## CONSTRAINTS
- Inventory only. No code changes this turn.
- Surface conflicts between the user's prompt mental model and the actual layer topology before refactoring.
- Exclude `tests/**` and `_archives/**`.

## OUTPUT FORMAT
Per red flag: location, pattern, severity, remediation pointer.
Summary table of engine handlers + process managers.
Conflicts / scope decisions the user must make before Phase 2+.

## VALIDATION CRITERIA
- Every aggregate file examined for the 4 purity red flags.
- Engine handler count enumerated by type.
- Process managers / sagas located.
- No code written.
