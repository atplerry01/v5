# reconciliation-run

**Classification:** control-system
**Context:** system-reconciliation
**Domain:** reconciliation-run

## Purpose

Represents a bounded execution of the full reconciliation pipeline over a named scope. A run is scheduled, started, and either completed (recording checks processed and discrepancies found) or aborted with a reason — providing operational visibility into reconciliation health over time.

## Aggregate: ReconciliationRunAggregate

| Property | Type | Description |
|---|---|---|
| Id | ReconciliationRunId | Deterministic 64-hex SHA256 identifier |
| Scope | string | The named reconciliation scope this run covers |
| Status | RunStatus | Pending / Running / Completed / Aborted |
| ChecksProcessed | int | Count of checks executed (set on completion) |
| DiscrepanciesFound | int | Count of discrepancies discovered (set on completion) |

## Invariants

- Scope must not be empty.
- Start() is only valid when status is Pending.
- Complete() and Abort() are only valid when status is Running.
- Abort() requires a non-empty reason.

## Events

| Event | Trigger |
|---|---|
| ReconciliationRunScheduledEvent | Run is registered with a scope and placed in Pending status |
| ReconciliationRunStartedEvent | Run transitions from Pending to Running |
| ReconciliationRunCompletedEvent | Run finishes successfully with outcome metrics |
| ReconciliationRunAbortedEvent | Run is terminated early with a reason |
