# discrepancy-detection

**Classification:** control-system
**Context:** system-reconciliation
**Domain:** discrepancy-detection

## Purpose

Models the identification and classification of a specific data or state discrepancy found during reconciliation. Each detection records the kind of discrepancy and its source reference, and can be dismissed with a reason when found to be a false positive.

## Aggregate: DiscrepancyDetectionAggregate

| Property | Type | Description |
|---|---|---|
| Id | DiscrepancyDetectionId | Deterministic 64-hex SHA256 identifier |
| Kind | DiscrepancyKind | MissingRecord / ExtraRecord / ValueMismatch / SequenceGap / IntegrityViolation |
| SourceReference | string | Reference to the system entity or record containing the discrepancy |
| Status | DetectionStatus | Detected / Dismissed / Escalated |
| DetectedAt | DateTimeOffset | Timestamp when the discrepancy was detected |

## Invariants

- SourceReference must not be empty.
- Dismiss() requires a non-empty reason.
- Dismiss() is only valid when status is Detected.

## Events

| Event | Trigger |
|---|---|
| DiscrepancyDetectedEvent | A discrepancy is identified and classified |
| DiscrepancyDetectionDismissedEvent | Detection is dismissed as a false positive with a reason |
