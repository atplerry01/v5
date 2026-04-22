# control-system / audit

**Classification:** control-system  
**Context:** audit

## Purpose
Owns the immutable record of system-significant events (audit log) and the structured findings raised against those records. The audit context is append-only by construction.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `audit-log` | Immutable log entries for system-significant events |
| `audit-record` | Structured records raised against audit log entries |

## Does Not Own
- Policy decisions (→ system-policy)
- Access control evaluation (→ access-control)
- Business-specific audit cases (deferred — those carry domain semantics)

## Inputs
- System event stream (from platform-system/event via runtime)
- Enforcement records (from system-policy/policy-enforcement)

## Outputs
- `AuditEntryRecordedEvent`
- `AuditRecordRaisedEvent`

## Invariants
- Audit entries are immutable — no updates, no deletes
- Every record references at least one audit log entry
- Record severity is one of: informational / warning / violation / critical

## Dependencies
- `core-system/identifier` — entry and finding IDs
- `core-system/temporal` — entry timestamps
