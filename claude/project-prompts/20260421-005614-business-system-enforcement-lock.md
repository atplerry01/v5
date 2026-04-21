---
classification: business-system
context: cross-context
domain: enforcement-lock
stored_at: 2026-04-21T00:56:14Z
---

# TITLE
Business-System Enforcement Lock Pass — regression-proofing via ArchTests

# CONTEXT
Immediately follows the Business-System Enforcement Pass (prompt `20260421-004717`) which eliminated primitive IDs, strengthened invariants, established a canonical DomainPolicy pattern, and confirmed no ambiguity paths. This pass exists to LOCK those gains so they cannot regress.

# OBJECTIVE
Produce text-based architectural tests (regex scans over `src/domain/business-system/**`) that fail fast when:
- any aggregate regresses to primitive IDs
- `EnsureInvariants()` becomes empty or trivial (identity-only)
- cross-aggregate rules appear inside aggregates
- events drift from `<Entity><Action>Event` naming or canonical-ID payloads
- DomainPolicy classes mutate state or reach for repositories

# CONSTRAINTS
- Do NOT change domain behavior.
- Do NOT modify aggregates unless strictly required to make an enforcement test pass.
- Do NOT introduce new abstractions.
- Text-based scans only (no Roslyn), matching the existing `StructuralEnforcementArchTests` / `WbsmArchitectureTests` style.

# EXECUTION STEPS
1. Create `tests/unit/domain/BusinessSystemEnforcementLockTests.cs` with five test blocks mapped 1:1 to the five lock phases.
2. Run the unit test project; all new tests must pass against current state.
3. Post-execution audit sweep per $1b.
4. Capture any drift per $1c.

# OUTPUT FORMAT
- One test file, five test methods (Phase 1–5 lock).
- Test run output showing all green.

# VALIDATION CRITERIA
- `dotnet test` runs green on the new lock suite.
- Tests are self-describing (each failure message names the rule and offenders).
- No modification to aggregates, events, policies, or VOs is required to make the suite pass (since enforcement pass already established compliance).
