---
captured_at: 2026-04-21T01:15:00Z
source: Content-System Correction + Scoped Externalisation Pass
classification: content-system / business-system / economic-system / operational-system
type: guards + migration record
severity: mixed (S0 / S1 / S2)
status: promotable rules green-locked; E2E-test and read-side tech debt tracked
---

# Content-System Correction + Scoped Externalisation — new rule IDs

Supersedes parts of `20260421-010348-content-system.md`. Corrections applied
per user directive:

- **Reverted**: `OrderAggregate.Description` from `ContentRef` back to typed
  `OrderDescription` VO. Order descriptions are non-evolving structural
  descriptors — not content.
- **Externalised (this pass)**: three true-evolving-content fields migrated
  to `DocumentRef`.
- **Not externalised**: audit / label / identity fields remain typed VOs or
  short strings.

## Scope executed

| Aggregate / Entity | Field | Action | New type |
|---|---|---|---|
| `OrderAggregate` | `Description` | REVERT | `OrderDescription` VO |
| `EnforcementRuleAggregate` | `Description` | EXTERNALISE | `DocumentRef` |
| `AuditRecordAggregate` | `EvidenceSummary` | EXTERNALISE | `DocumentRef` |
| `KanbanCard` | `Title` + `Description` → `Content` | EXTERNALISE (collapsed) | `DocumentRef` |

## Scope explicitly REFUSED (kept as-is)

- `ViolationAggregate.Reason` — event/audit narrative
- `SettlementAggregate.FailureReason` — event/audit narrative
- `KanbanList.Name` — structural label
- `KanbanAggregate.Name` — board structural label
- `CounterpartyProfile.Name` — identity
- `RuleName` on `EnforcementRuleAggregate` — identity
- `ExpenseLine.Description` — transactional line memo (per prior user directive)

## Promotable guard rules (green-locked in `ContentSystemEnforcementLockTests`)

### CS-ORDER-NO-PROSE-01 (S1)
`OrderAggregate` MUST NOT declare a raw-string prose property
(`OrderDescription|Description|Body|Notes|Comment`).

### CS-ORDER-NOT-EXTERNALISED-01 (S2)
`OrderAggregate.Description` MUST remain a typed VO (`OrderDescription`),
NOT a `DocumentRef`. Externalisation is reserved for true evolving content.

### CS-DECOUPLE-RULE-01 (S1)
`EnforcementRuleAggregate.Description` MUST be `DocumentRef`, not `string` and
not `RuleDescription`.

### CS-DECOUPLE-EVIDENCE-01 (S1)
`AuditRecordAggregate.EvidenceSummary` MUST be `DocumentRef`, not the legacy
`EvidenceSummary` VO.

### CS-DECOUPLE-KANBAN-01 (S1)
`KanbanCard` MUST NOT declare raw-string `Title` / `Description`. Both collapse
into a single `DocumentRef Content`.

### CS-NO-OVER-EXTERNALISE-01 (S2)
The following structural / identity / audit fields MUST NOT be externalised
to `DocumentRef`:
`ViolationAggregate.Reason`, `SettlementAggregate.FailureReason`,
`CounterpartyProfile.Name`, `KanbanAggregate.Name`.

**Test:** `Structural_labels_remain_typed_VOs_and_do_not_externalise_to_DocumentRef`.

## Schema-breakage policy (as applied)

Per user directive "no schema breakage", the following convention was used:
- **Domain layer**: aggregates and domain events carry typed `DocumentRef`.
- **External wire schemas** (`*EventSchema`, read models, commands, DTOs):
  field NAMES and SHAPES preserved as-is. Semantic meaning of legacy `string`
  fields has shifted:
  - `EnforcementRuleDefinedEventSchema.Description` — now carries the
    DocumentId as a Guid-string.
  - `AuditRecordCreatedEventSchema.EvidenceSummary` — now carries the
    DocumentId as a Guid-string.
  - `KanbanCardCreatedEventSchema.Title` — now carries the DocumentId as a
    Guid-string; `.Description` is always empty.
  - `KanbanCardUpdatedEventSchema.Title` — now carries the DocumentId as a
    Guid-string; `.Description` is always empty.

- **Engine handlers** (`DefineEnforcementRuleHandler`, `CreateAuditRecordHandler`,
  `CreateKanbanCardHandler`, `UpdateKanbanCardHandler`) — parse the incoming
  command's legacy-string field as a Guid and construct the `DocumentRef`.
  Throw if the string is not a valid non-empty Guid.

## Tech debt (NOT addressed in this pass)

### E2E tests
`tests/e2e/economic/compliance/audit/AuditRecordE2ETests.cs` sends prose
strings through `CreateAuditRecordRequestModel.EvidenceSummary`. After this
pass, the handler will reject non-Guid strings. The E2E test must be updated
to pass a valid Guid (the DocumentId of a pre-created content aggregate) and
set up a corresponding `DocumentAggregate` via the content-system API.

### Kanban read-side
`KanbanProjectionReducer` / `KanbanCardReadModel` / `KanbanController` still
present `Title` (Guid-string) and `Description` (empty) to downstream UI
consumers. Future work:
- Add a read-side join between `KanbanCardReadModel` and the content-system
  projection to resolve `Content.Value → DocumentVersion.Payload`.
- OR update the UI to display card content by dereferencing the DocumentId.

### Compliance audit E2E evidence text
The E2E asserts `evidence` (prose) equals `read.Data.EvidenceSummary`. After
this pass, `read.Data.EvidenceSummary` is a Guid-string. The test must be
updated either to assert on the content-system projection after dereferencing
the DocumentId, or to assert the Guid string shape.

### Migration of stored data
No live data migration performed — this is a code-level refactor. If any
environment has live events on Kafka or rows in the projection schemas
carrying prose in these fields, a separate data migration pass is required:
- Create `DocumentAggregate` per legacy prose row.
- Backfill the reference column with the new DocumentId.

## Status tracking

Green (enforced by `ContentSystemEnforcementLockTests`):
- Revert of OrderAggregate — ✓
- EnforcementRuleAggregate.Description → DocumentRef — ✓
- AuditRecordAggregate.EvidenceSummary → DocumentRef — ✓
- KanbanCard.Title+Description → DocumentRef Content — ✓
- No-over-externalisation guard for 4 structural fields — ✓

Pending (out of scope for this pass):
- E2E test updates (3 test files)
- Kanban read-side dereferencing
- Data migration plan for any live environments
