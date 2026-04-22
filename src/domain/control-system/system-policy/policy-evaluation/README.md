# policy-evaluation

**Classification:** control-system
**Context:** system-policy
**Domain:** policy-evaluation

## Purpose

Records the lifecycle of a single policy evaluation: the request (policy, actor, action, correlation) and the resulting verdict (Allow / Deny / Defer with a tamper-evident decision hash). Policy evaluation is the domain record of every WHYCEPOLICY check; the decision hash provides the chain-anchorable evidence trail.

## Aggregate: PolicyEvaluationAggregate

| Property | Type | Description |
|---|---|---|
| Id | PolicyEvaluationId | Deterministic 64-hex SHA256 identifier |
| PolicyId | string | The policy that was evaluated |
| ActorId | string | The actor requesting the action |
| Action | string | The action being checked |
| CorrelationId | string | Execution correlation identifier |
| Outcome | EvaluationOutcome? | Allow / Deny / Defer (null until verdict issued) |
| DecisionHash | string? | SHA256 of decision inputs for audit anchoring |

## Invariants

- PolicyId, ActorId, and Action must not be empty.
- Verdict may only be issued once.
- DecisionHash must not be empty when verdict is issued.

## Events

| Event | Trigger |
|---|---|
| PolicyEvaluationRecordedEvent | Evaluation request received |
| PolicyEvaluationVerdictIssuedEvent | Allow / Deny / Defer outcome determined |
