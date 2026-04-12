# Domain: Compliance

## Classification
economic-system

## Context
compliance

## Purpose
Audit and compliance layer for the economic classification, ensuring that economically significant actions are evidencable, reviewable, and traceable for governance and external reporting. Compliance records evidence — it does not define financial truth and does not alter financial state.

## Core Responsibilities
- Recording audit-grade compliance evidence for source actions
- Finalizing audit records to seal evidence (immutable after finalization)
- Ensuring audit records are traceable, durable, and reviewable

## Aggregate(s)
- **AuditRecordAggregate** (`audit/`)
  - Event-sourced, sealed. Manages audit evidence capture and finalization
  - Invariants: Must reference source action (A1); immutable after finalization (A2); must include evidence summary for review (A3); cannot finalize twice

## Entities
None

## Value Objects
- **AuditRecordId** — Typed Guid wrapper for unique audit record identity
- **AuditRecordStatus** — Enum: Draft, Finalized
- **AuditType** — Enum: Financial, Control, Violation, Reconciliation, Settlement
- **EvidenceSummary** — Human-readable audit context (cannot be empty/whitespace)
- **SourceAggregateId** — Cross-domain reference to source aggregate
- **SourceDomain** — Origin domain classification (string, cannot be empty)
- **SourceEventId** — Cross-domain reference to source event

## Domain Events
- **AuditRecordCreatedEvent** — Audit evidence captured for source action (A1 requires source reference)
- **AuditRecordFinalizedEvent** — Audit evidence sealed and immutable (A2 terminal)

## Specifications
- **AuditRecordSpecification** — CanFinalize (status is Draft); IsFinalized (status is Finalized)

## Domain Services
- **AuditService** — Stub implementation ready for domain logic

## Invariants (CRITICAL)
- A1: Every audit record must reference a source action or state
- A2: Audit records must be immutable after finalization
- A3: Audit records must include enough context for review
- A4: Compliance evidence must not be orphaned
- A5: Compliance cannot alter financial state

## Policy Dependencies
- Source reference enforcement (A1)
- Immutability enforcement after finalization (A2)
- Evidence sufficiency (A3)
- Non-mutative constraint — compliance never alters financial state (A5)

## Integration Points
- **capital** — Records capital evidence
- **ledger** — Records ledger evidence
- **transaction** — Records transaction evidence
- **revenue** — Records revenue evidence
- **enforcement** — Records enforcement evidence

## Lifecycle
```
CreateRecord() -> Draft (requires source reference and evidence summary)
  FinalizeRecord() -> Finalized (immutable, terminal)
```

## Notes
- Pure domain — zero runtime, infrastructure, or engine dependencies
- AuditErrors uses string constants (not factory methods)
- Compliance is an observer — it never modifies financial state in any context
