# audit-event

**Classification:** control-system
**Context:** audit
**Domain:** audit-event

## Purpose

An atomic, discrete event that forms the unit of audit evidence. Each captured audit-event records who performed what action, under which correlation, and when — providing the foundational data that audit-trace aggregates and audit-query surfaces. Events are sealed with an integrity hash to prevent tampering.

## Aggregate: AuditEventAggregate

| Property | Type | Description |
|---|---|---|
| Id | AuditEventId | Deterministic 64-hex SHA256 identifier |
| ActorId | string | Identity that performed the action |
| Action | string | The action performed |
| Kind | AuditEventKind | AccessDecision / PolicyEvaluation / ConfigurationChange / IdentityAction / SystemAction / SecurityIncident |
| CorrelationId | string | Execution correlation for trace linkage |
| OccurredAt | DateTimeOffset | When the event occurred |
| IsSealed | bool | Whether the event has been integrity-sealed |
| IntegrityHash | string? | SHA256 of event content for tamper evidence |

## Invariants

- ActorId, Action, and CorrelationId must not be empty.
- A sealed event cannot be re-sealed.

## Events

| Event | Trigger |
|---|---|
| AuditEventCapturedEvent | Discrete audit observation captured |
| AuditEventSealedEvent | Event integrity-sealed with hash |
