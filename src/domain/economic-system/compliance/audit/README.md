# Domain: Audit

## Classification
economic-system

## Context
compliance

## Domain Group
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
- **AuditService** — Orchestrates record creation and finalize, enforcing the `AuditRecordSpecification.CanFinalize` precondition.

## Invariants (CRITICAL)
- Every audit record must reference a source action (A1)
- Audit records must be immutable after finalization (A2)
- Audit records must include a description for review context (A3)
- Source aggregate id and source event id must be non-empty Guids
- Source domain string must be non-empty
- Evidence summary must be non-whitespace
- Only Draft records can be finalized
- Cannot finalize twice

## Policy Dependencies (E5)
- `whyce.economic.compliance.audit.create` — create audit record
- `whyce.economic.compliance.audit.finalize` — finalize audit record
- `whyce.economic.compliance.audit.read` — read audit record

## Integration Points
- All economic contexts — Audit records capture evidence from capital, ledger, transaction, revenue, and enforcement contexts via cross-domain source references

## Event Fabric (E6)
- Topic: `whyce.economic.compliance.audit.events`
- Consumer group: `whyce.projection.economic.compliance.audit`
- Schemas live under `Whycespace.Shared.Contracts.Events.Economic.Compliance.Audit`
- Payload mappers registered in `EconomicSchemaModule.RegisterComplianceAudit`

## Projection (E7)
- Read model: `AuditRecordReadModel` (`projection_economic_compliance_audit.audit_record_read_model`)
- Handler: `AuditRecordProjectionHandler` (inline execution)
- Reducer: `AuditRecordProjectionReducer`

## API Surface (E8)
- `POST /api/compliance/audit/create`
- `POST /api/compliance/audit/finalize`
- `GET  /api/compliance/audit/{id}`

## Lifecycle
```
CreateRecord() -> Draft (requires source references and evidence summary)
  FinalizeRecord() -> Finalized (immutable, terminal)
```

## Observability (E10)
- Emit `AuditRecordCreatedEvent` / `AuditRecordFinalizedEvent` to the fabric (already enforced by `DomainEvents`).
- Recommended metrics: `audit.record.created.count`, `audit.record.finalized.count`, `audit.record.finalize_latency_seconds`.
- Tracing path: API → Runtime.Dispatcher → T2E.CreateAuditRecordHandler → Aggregate → Outbox → Kafka → Projection.

## Security / Enforcement (E11)
- API surface requires `[Authorize]`; per-command policy enforcement is applied through `PolicyMiddleware` using the canonical policy ids.
- Audit is observational only — no counter-mutations to source aggregates are performed.

## E2E Validation (E12)
- POST `/api/compliance/audit/create` → 200 `audit_record_created`
- Verify row present in `projection_economic_compliance_audit.audit_record_read_model` with `Status = "Draft"`
- Verify event on Kafka topic `whyce.economic.compliance.audit.events` with type `AuditRecordCreatedEvent`
- POST `/api/compliance/audit/finalize` → 200 `audit_record_finalized`
- Verify projection row transitions to `Status = "Finalized"` and `FinalizedAt` set
- Verify `AuditRecordFinalizedEvent` appears on the same topic
- Re-issuing finalize on the same record → 400 `economic.compliance.audit.finalize_failed`

## Notes
- Workflow layer (E9) is intentionally NOT implemented — audit is a simple two-state bounded domain; no orchestration is justified.
- AuditErrors uses string constants rather than factory methods (locked convention for this domain)
- Compliance is an observer — it never modifies financial state
- Cross-domain references (SourceAggregateId, SourceEventId) use Guid wrappers
- SourceDomain is a string-based classification (not a typed reference)
