# audit-trace

**Classification:** control-system
**Context:** audit
**Domain:** audit-trace

## Purpose

A correlated chain of audit events forming a complete execution trace. An audit-trace is opened at the start of a governed operation and collects references to discrete audit-events throughout its span. Closing the trace finalises the chain of evidence for that correlation scope.

## Aggregate: AuditTraceAggregate

| Property | Type | Description |
|---|---|---|
| Id | AuditTraceId | Deterministic 64-hex SHA256 identifier |
| CorrelationId | string | The correlation scope this trace covers |
| OpenedAt | DateTimeOffset | When the trace was opened |
| ClosedAt | DateTimeOffset? | When the trace was closed (null if still open) |
| LinkedAuditEventIds | IReadOnlyList&lt;string&gt; | Ordered references to audit-events in this trace |
| Status | TraceStatus | Open / Closed |

## Invariants

- CorrelationId must not be empty.
- Events cannot be linked to a closed trace.
- A closed trace cannot be re-closed.

## Events

| Event | Trigger |
|---|---|
| AuditTraceOpenedEvent | Trace opened for a correlation scope |
| AuditTraceEventLinkedEvent | Audit event added to the trace |
| AuditTraceClosedEvent | Trace finalised |
