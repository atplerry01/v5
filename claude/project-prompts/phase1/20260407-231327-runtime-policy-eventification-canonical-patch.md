# WBSM v3.5 — POLICY EVENTIFICATION (CANONICAL PATCH)

CLASSIFICATION: runtime / constitutional-system / policy
STORED: 2026-04-07 23:13:27

## CONTEXT

Promotes `claude/new-rules/20260407-190000-policy-eventification.md` from backlog
into a canonical patch. The Option-4 ALLOW pass and the BLOCKER 1/3/4 resolutions
already landed prior AuditEmission, dedicated audit stream + topic, and
PolicyMiddleware deny-path emission. This patch closes the residual deltas
between current state and the prompt's literal acceptance criteria.

## OBJECTIVE

Implement canonical policy eventification so that:
- ALL policy decisions (ALLOW + DENY) emit AuditEmission
- Audit events route to a dedicated aggregate stream
- Audit flow is fully isolated from domain event flow
- Chain anchoring + Kafka publishing occur for audit events FIRST

## CONSTRAINTS

- WHYCEPOLICY enforcement doctrine, no bypass
- Deterministic execution (no timestamps in hashes, no Guid.NewGuid)
- Runtime layer forbidden from referencing Whycespace.Domain.* (rule 11.R-DOM-01)
- ControlPlane → Fabric is single, non-bypassable
- No mixed-stream logic, no IsAuditEmission flag

## EXECUTION STEPS

1. AuditEmission: add Metadata (DecisionHash, ExecutionHash, PolicyVersion, CommandId)
2. IPolicyDecisionEventFactory: return AuditEmission directly
3. PolicyDecisionEventFactory (engine): build AuditEmission with routing + metadata
4. PolicyMiddleware: delegate to factory; ALLOW path always emits regardless of inner result
5. Tests: existing PolicyEventificationTests cover ordering + isolation
6. Build + tests

## OUTPUT FORMAT

Files modified, before/after summary, build/test verification, deviations.

## VALIDATION CRITERIA

- AuditEmission populated for ALL policy decisions
- Audit persisted/published BEFORE domain events
- Dedicated stream + dedicated topic enforced
- DENY blocks dispatcher, ALLOW continues pipeline
- Tests pass

## DEVIATIONS (PRE-DECLARED)

- AuditEmission.AggregateId remains `Guid` (not literal `string` per prompt) because EventEnvelope.AggregateId is Guid downstream; the value is still derived from the deterministic seed `policy-audit-stream:{CommandId}` via IIdGenerator.
- Factory takes the pre-derived aggregateId from caller (avoids injecting IIdGenerator into the engine factory; preserves its purity).
