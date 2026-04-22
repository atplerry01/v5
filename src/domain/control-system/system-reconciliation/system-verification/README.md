# system-verification

**Classification:** control-system
**Context:** system-reconciliation
**Domain:** system-verification

## Purpose

Models a targeted verification of a named system's state or integrity. A verification is initiated, then either passes (confirming correctness) or fails (recording the failure reason) — making system-level integrity assertions a first-class, event-sourced domain fact.

## Aggregate: SystemVerificationAggregate

| Property | Type | Description |
|---|---|---|
| Id | SystemVerificationId | Deterministic 64-hex SHA256 identifier |
| TargetSystem | string | The system or component being verified |
| Status | VerificationStatus | Initiated / Passed / Failed |
| FailureReason | string? | Set only when status is Failed |
| InitiatedAt | DateTimeOffset | Timestamp when verification was initiated |

## Invariants

- TargetSystem must not be empty.
- Fail() requires a non-empty failure reason.
- Pass() and Fail() are only valid when status is Initiated.

## Events

| Event | Trigger |
|---|---|
| SystemVerificationInitiatedEvent | Verification begins for a target system |
| SystemVerificationPassedEvent | Verification confirms system state is correct |
| SystemVerificationFailedEvent | Verification detects an integrity or state failure |
