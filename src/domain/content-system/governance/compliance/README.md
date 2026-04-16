# Compliance Check Domain

**Path:** `content-system/governance/compliance`
**Namespace:** `Whycespace.Domain.ContentSystem.Governance.Compliance`

## Purpose
Runs a single compliance check of a subject against a named rule and
records the outcome.

## Lifecycle
```
Initiated → Passed (terminal)
         → Failed (terminal)
         → Expired (terminal)
```

## Events
- `ComplianceCheckInitiatedEvent`
- `ComplianceCheckPassedEvent`
- `ComplianceCheckFailedEvent`
- `ComplianceCheckExpiredEvent`

## Invariants
1. Subject reference required.
2. Only Initiated may transition to terminal outcomes.
3. Passed/Failed/Expired are terminal.
