# TITLE
Enterprise-grade runtime upgrade — close ABSENT and PARTIAL features

## CONTEXT
User reviewed the runtime feature spec at `claude/project-topics/v2b/runtime.md` (23 sections, ~340 features) and the gap matrix at `claude/project-topics/v2b/runtime-gap-matrix.md` (197 PRESENT / 101 PARTIAL / 93 ABSENT). The runtime's happy path is ~50% enterprise-complete; the unhappy path (§14 Failure Handling, §11 Event Fabric DLQ/rebalance, §20 Backpressure circuit-breakers, §21 Admin Controls, §23 Testing) is where most gaps cluster.

## OBJECTIVE
Bring the runtime to enterprise-grade completeness by systematically closing every ABSENT and PARTIAL feature, organized into a phased upgrade plan with checkpoints so architectural decisions can be ratified by the user.

## CONSTRAINTS
- Canonical 11-stage pipeline is LOCKED per `RO-CANONICAL-11` — no reordering, no new stages, no optional middlewares.
- R11 — runtime carries no domain logic; no `Whycespace.Domain.*` imports except in `src/runtime/event-fabric/domain-schemas/**` and `RuntimeCommandDispatcher.cs` kernel-primitive exception.
- Determinism rules: no `Guid.NewGuid`, no `DateTime.UtcNow`, only `IIdGenerator`/`IClock` — extends to new code.
- WHYCEPOLICY gates every mutation path; new features requiring mutation must register policies.
- R7 persist/publish/anchor authority stays in runtime only; engines emit events.
- Outbox pattern is mandatory (R14); no direct external publishing inside command transaction.
- $5 Anti-drift: no architecture changes or pattern introductions without explicit user approval.

## EXECUTION STEPS
1. Produce the phased upgrade plan document at `claude/project-topics/v2b/runtime-upgrade-plan.md`.
2. For each phase: list in-scope features, entry/exit criteria, architectural decisions required, and guard/audit impact.
3. Present plan; obtain user sign-off on R1 scope before any code changes.
4. Execute phases sequentially with checkpoints between each.
5. Run post-execution audit sweep per $1b at each checkpoint.
6. Capture any new drift rules discovered during execution into `claude/new-rules/` per $1c.

## OUTPUT FORMAT
- Phased plan document (markdown) covering R1 through R5.
- Per-phase: scope, effort estimate, architectural decisions, entry/exit criteria.
- A decision log section cataloguing the open architectural choices that require user input before R2.

## VALIDATION CRITERIA
- Plan maps each of the 93 ABSENT + 101 PARTIAL rows in the gap matrix to exactly one phase.
- Phases honor dependency order (foundation → resilience → workflow → ops → certification).
- Architectural decisions are flagged, not silently made.
- Exit criteria include guard rule IDs and audit checklist updates so closure is measurable under the $1a/$1b regime.

## CLASSIFICATION
- Layer: runtime
- Context: control-plane, event-fabric, workflow, observability, testing
- Domain: (N/A — runtime layer work, not domain layer)
- Severity: S1 (architectural)
