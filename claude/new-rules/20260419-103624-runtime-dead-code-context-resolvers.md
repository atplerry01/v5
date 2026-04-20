---
captured: 2026-04-19T10:36:24
captured-by: economic-vo-hardening session (post-execution audit sweep per $1b)
type: runtime
severity: S1
status: open
---

# Sibling context resolvers are dead code (runtime no-dead-code subsystem)

## CLASSIFICATION

Runtime layer code-quality violation — `runtime.guard.md` no-dead-code subsystem (Dead Code R1).

## SOURCE

Discovered during the post-execution audit sweep on 2026-04-19, immediately after deletion of the dead `EconomicContextResolver`. The deletion grep that confirmed `EconomicContextResolver` had zero callers also confirmed the same pattern for two siblings.

## DESCRIPTION

The following files under [src/runtime/context/](../../src/runtime/context/) appear to have **zero callers** anywhere in `src/`, `tests/`, `infrastructure/`, or `scripts/`:

1. `src/runtime/context/IdentityContextResolver.cs`
2. `src/runtime/context/WorkflowContextResolver.cs`

Verified via `grep -rEn 'IdentityContextResolver|WorkflowContextResolver' src/ tests/ infrastructure/ scripts/` — only two file definition matches, no usage matches.

Per `runtime.guard.md` no-dead-code subsystem, confirmed zero-caller code MUST be deleted, not preserved "in case it's needed later".

## PROPOSED_RULE

This is enforcement of an existing rule, not a new rule. Three actions:

1. **Verify zero callers a second time** before deletion (the pattern is too broad to assume — the names could appear in reflection-driven DI registration, OpenTelemetry interceptors, or test discovery).
2. **If confirmed dead**: delete both files in a single PR alongside any other dead code discovered by a broader sweep of `src/runtime/context/` and `src/runtime/middleware/`.
3. **If a planned consumer exists** (e.g., a future Identity vertical needs `IdentityContextResolver`): leave in place and add a `// TODO(phase-2.8): wired by WhyceID composition root` comment. Otherwise the file fails the no-dead-code rule.

The third option is suspect — `runtime.guard.md` discourages speculative scaffolding ("no inference of missing components"). Prefer deletion until the consumer exists.

## SEVERITY

**S1** — runtime layer purity / dead-code constraint. Two files. Not blocking economic core operation but accumulates if not addressed.

## OUT-OF-SCOPE JUSTIFICATION

Sibling files to `EconomicContextResolver` which WAS deleted in this session. The session's scope explicitly bounded the deletion to the economic resolver only ("Delete dead `EconomicContextResolver`"). Expanding scope mid-session to delete sibling resolvers would have violated CLAUDE.md $5 anti-drift ("no architecture changes" extends to "no expanding agreed scope mid-session"). Captured here for follow-up.

## PROMOTION CANDIDATE

Promote into the next runtime cleanup PR. Suggested follow-up sweep: enumerate all `src/runtime/**/*Resolver.cs`, `src/runtime/**/*Validator.cs`, and `src/runtime/**/*Helper.cs` and grep each for callers; delete or document any with zero callers.
