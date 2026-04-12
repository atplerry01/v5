# Domain: Audit

## Classification
economic-system

## Context
compliance

## Purpose
Records audit-grade compliance evidence for economically significant actions. Audit records are durable, traceable, and immutable after finalization. The audit domain observes — it never modifies financial state.

## Core Responsibilities
- Creating audit records linked to source actions
- Finalizing audit records to seal evidence
- Ensuring audit records are traceable and reviewable

## Aggregate(s)
- **AuditRecordAggregate**
  - Event-sourced, sealed. Manages audit evidence lifecycle from creation to finalization
  - Invariants: Must reference source domain, aggregate, and event; must include evidence summary; cannot finalize twice; only Draft can transition to Finalized

## Entities
None

## Value Objects
- **AuditRecordId** — Typed Guid wrapper for unique audit record identity
- **AuditRecordStatus** — Enum: Draft, Finalized
- **AuditType** — Enum: Financial, Control, Violation, Reconciliation, Settlement
- **EvidenceSummary** — Human-readable audit context (cannot be empty/whitespace)
- **SourceAggregateId** — Cross-domain reference to source aggregate (cannot be empty)
- **SourceDomain** — Origin domain classification string (cannot be empty/whitespace)
- **SourceEventId** — Cross-domain reference to source event (cannot be empty)

## Domain Events
- **AuditRecordCreatedEvent** — Audit evidence captured (captures source domain, aggregate, event, type, summary)
- **AuditRecordFinalizedEvent** — Audit evidence sealed and immutable

## Specifications
- **AuditRecordSpecification** — CanFinalize (status is Draft); IsFinalized (status is Finalized)

## Domain Services
- **AuditService** — Stub implementation ready for domain logic

## Invariants (CRITICAL)
- Every audit record must reference a source action (A1)
- Audit records must be immutable after finalization (A2)
- Audit records must include a description for review context (A3)
- Source action ID must be non-empty
- Only Draft records can be finalized
- Cannot finalize twice

## Policy Dependencies
- Source reference enforcement (A1)
- Immutability enforcement after finalization (A2)
- Evidence sufficiency — must include evidence summary (A3)

## Integration Points
- All economic contexts — Audit records capture evidence from capital, ledger, transaction, revenue, and enforcement contexts via cross-domain source references

## Lifecycle
```
CreateRecord() -> Draft (requires source references and evidence summary)
  FinalizeRecord() -> Finalized (immutable, terminal)
```

## Notes
- AuditErrors uses string constants rather than factory methods
- Compliance is an observer — it never modifies financial state
- Cross-domain references (SourceAggregateId, SourceEventId) use Guid wrappers
- SourceDomain is a string-based classification (not a typed reference)
