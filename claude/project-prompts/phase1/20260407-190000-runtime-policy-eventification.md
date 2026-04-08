# POLICY EVENTIFICATION — STRICT WBSM v3.5

CLASSIFICATION: runtime
CONTEXT: constitutional-system
DOMAIN: policy/decision (new sub-domain)

## CONTEXT
Policy decisions produce DecisionHash and chain-anchor it, but no domain events
exist for policy decisions. Policy is not observable via EventStore, not
replayable as events, not projectable. Audit trail is incomplete.

## OBJECTIVE
Upgrade POLICY from "Decision → Hash → Chain" to
"Decision → Domain Event → EventStore → Chain → Kafka → Projection" without
breaking determinism, runtime pipeline, guards, or introducing drift.

## NON-NEGOTIABLE RULES
1. APPEND-ONLY for guards/audits
2. NO modification of existing domain behavior beyond policy emission
3. Events MUST go through RuntimeControlPlane pipeline
4. NO direct persistence from middleware
5. Determinism preserved (no clock/random inside event)
6. DO NOT change existing PolicyHash generation
7. DO NOT duplicate events across layers

## EXECUTION STEPS
1. Create PolicyEvaluatedEvent + PolicyDeniedEvent in
   src/domain/constitutional-system/policy/decision/event/
2. Emit events from PolicyMiddleware via runtime event buffer
3. Register events in EventSchemaRegistry / TopicNameResolver
4. Verify pipeline (UnitOfWork, EventStore.Append, ChainAnchor, Kafka)
5. Replay compatibility — no policy re-evaluation on replay
6. Projection compatibility (no new projections)
7. Tests: allow / deny / determinism
8. Capture new guard rules (POLICY-EVENT-REQUIRED-01, POLICY-NO-SILENT-DECISION-01,
   POLICY-PIPELINE-INTEGRATION-01, POLICY-REPLAY-INTEGRITY-01)

## OUTPUT FORMAT
New domain events, modified PolicyMiddleware, registry wiring, tests, new-rules file.

## VALIDATION CRITERIA
Policy decisions produce events; events persisted, anchored, published; replay
deterministic; no runtime bypass.
