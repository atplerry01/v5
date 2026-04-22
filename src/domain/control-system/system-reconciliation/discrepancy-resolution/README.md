# discrepancy-resolution

**Classification:** control-system
**Context:** system-reconciliation
**Domain:** discrepancy-resolution

## Purpose

Tracks the resolution lifecycle for a confirmed discrepancy. A resolution is initiated against a detection reference, then completed with an outcome (Corrected / Accepted / Rejected) and mandatory resolution notes — providing an auditable record of how each discrepancy was handled.

## Aggregate: DiscrepancyResolutionAggregate

| Property | Type | Description |
|---|---|---|
| Id | DiscrepancyResolutionId | Deterministic 64-hex SHA256 identifier |
| DetectionReferenceId | string | The detection ID this resolution addresses |
| Status | ResolutionStatus | Initiated / Completed / Aborted |
| Outcome | ResolutionOutcome? | Corrected / Accepted / Rejected — set on completion |
| InitiatedAt | DateTimeOffset | Timestamp when resolution was initiated |

## Invariants

- DetectionReferenceId must not be empty.
- Complete() requires non-empty resolution notes.
- Complete() and Abort() are only valid when status is Initiated.

## Events

| Event | Trigger |
|---|---|
| DiscrepancyResolutionInitiatedEvent | Resolution process begins for a detected discrepancy |
| DiscrepancyResolutionCompletedEvent | Resolution finishes with an outcome and notes |
