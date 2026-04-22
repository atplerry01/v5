# audit-query

**Classification:** control-system
**Context:** audit
**Domain:** audit-query

## Purpose

Represents the intent and lifecycle of a query issued against audit data. An audit-query captures who queried, over which time range, with which filters, and what result count was returned — making the act of accessing audit evidence itself an auditable event.

## Aggregate: AuditQueryAggregate

| Property | Type | Description |
|---|---|---|
| Id | AuditQueryId | Deterministic 64-hex SHA256 identifier |
| IssuedBy | string | Identity that issued the query |
| TimeRange | QueryTimeRange | From / To window for the query |
| CorrelationFilter | string? | Optional correlation scope filter |
| ActorFilter | string? | Optional actor scope filter |
| Status | AuditQueryStatus | Issued / Completed / Expired |
| ResultCount | int? | Number of results returned (set on completion) |

## Invariants

- IssuedBy must not be empty.
- TimeRange To must be after From.
- Completion requires Issued status.

## Events

| Event | Trigger |
|---|---|
| AuditQueryIssuedEvent | Query submitted against audit data |
| AuditQueryCompletedEvent | Query finished with result count |
| AuditQueryExpiredEvent | Query timed out before completion |
