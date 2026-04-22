# policy-audit

**Classification:** control-system
**Context:** system-policy
**Domain:** policy-audit

## Purpose

Records append-safe policy audit entries for every governed policy event: evaluation passes, denials, deferrals, package deployments, and policy definition changes. Provides the queryable, tamper-evident evidence trail required for compliance and forensic review.

## Aggregate: PolicyAuditAggregate

| Property | Type | Description |
|---|---|---|
| Id | PolicyAuditId | Deterministic 64-hex SHA256 identifier |
| PolicyId | string | The policy referenced in this audit entry |
| ActorId | string | The actor associated with the event |
| Action | string | The action being audited |
| Category | PolicyAuditCategory | Classification of the audit event |
| DecisionHash | string | Hash of the policy decision for chain anchoring |
| CorrelationId | string | Execution correlation identifier |
| OccurredAt | DateTimeOffset | When the event occurred |
| IsReviewed | bool | Whether a human has reviewed this entry |

## Invariants

- PolicyId and ActorId must not be empty.
- An entry may only be reviewed once.
- Review requires a non-empty reason.

## Events

| Event | Trigger |
|---|---|
| PolicyAuditEntryRecordedEvent | Governed policy event captured as an audit entry |
| PolicyAuditEntryReviewedEvent | Entry manually reviewed by an authorized auditor |
