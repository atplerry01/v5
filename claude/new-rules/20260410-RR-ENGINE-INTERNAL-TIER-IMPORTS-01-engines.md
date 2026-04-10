CLASSIFICATION: engines
SOURCE: deep-sweep-20260410.audit.output.md (subagent: determinism/engine/runtime)
SEVERITY: S2

DESCRIPTION:
engine.guard RULE 7 ("NO ENGINE-TO-ENGINE IMPORTS") is currently ambiguous about
same-tier internal imports. WorkflowEngine.cs:4-5 imports
`Whyce.Engines.T1M.Lifecycle` and `Whyce.Engines.T1M.StepExecutor` — these are
T1M-internal helpers, not cross-tier T1M↔T2E coupling. Audit subagent flagged it
as S1 then cleared it on review, which is exactly the kind of ambiguity a rule
should eliminate.

PROPOSED_RULE:
Amend engine.guard RULE 7 to:
"Cross-tier engine imports are FORBIDDEN (T0U ↔ T1M ↔ T2E). Same-tier internal
helper imports (e.g. T1M.WorkflowEngine importing T1M.Lifecycle, T1M.StepExecutor)
are PERMITTED provided the imported namespace shares the tier prefix.
Lint: `using Whyce.Engines.<TIER>.*;` is allowed only inside files under
`src/engines/<TIER>/**`."

PROMOTION TARGET: claude/guards/engine.guard.md
