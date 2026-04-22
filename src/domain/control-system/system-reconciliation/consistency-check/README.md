# consistency-check

**Classification:** control-system
**Context:** system-reconciliation
**Domain:** consistency-check

## Purpose

Represents a scoped consistency evaluation over a named system target. A consistency-check aggregate is initiated against a specific scope, runs to completion, and records whether discrepancies were found — forming the trigger point for downstream discrepancy-detection and discrepancy-resolution flows.

## Aggregate: ConsistencyCheckAggregate

| Property | Type | Description |
|---|---|---|
| Id | ConsistencyCheckId | Deterministic 64-hex SHA256 identifier |
| ScopeTarget | string | The named system scope being checked |
| Status | ConsistencyCheckStatus | Initiated / Completed / Failed |
| HasDiscrepancies | bool? | Set on completion; null while in progress |
| InitiatedAt | DateTimeOffset | Timestamp when the check was initiated |

## Invariants

- ScopeTarget must not be empty.
- Complete() is only valid when status is Initiated.

## Events

| Event | Trigger |
|---|---|
| ConsistencyCheckInitiatedEvent | Check started against a scope target |
| ConsistencyCheckCompletedEvent | Check finished, recording whether discrepancies exist |
