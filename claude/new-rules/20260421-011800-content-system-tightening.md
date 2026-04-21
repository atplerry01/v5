---
captured_at: 2026-04-21T01:18:00Z
source: Content-System Final Tightening Pass
classification: content-system / shared-kernel
type: guards + type system
severity: mixed (S1 / S2)
status: promotable rules green-locked
---

# Content-System Tightening — ContentId + DocumentRef misuse guard

Builds on `20260421-011500-content-system-correction.md`. Adds strong typing
for content identity and a misuse guard against accidental externalisation of
identity / label fields.

## Changes applied

### 1. Canonical ContentId VO

- **New**: `src/domain/shared-kernel/primitive/identity/ContentId.cs` — record
  struct wrapping a `Guid` with `TryParse` helper and fail-fast on empty.
- Single canonical identity type for all content aggregates, reachable by any
  domain via the shared-kernel primitive namespace.

### 2. DocumentRef re-typed to wrap ContentId

All three local `DocumentRef` VOs now wrap `ContentId` instead of raw `Guid`:

- `economic-system/enforcement/rule/value-object/DocumentRef.cs`
- `economic-system/compliance/audit/value-object/DocumentRef.cs`
- `operational-system/sandbox/kanban/value-object/DocumentRef.cs`

`DocumentRef.Value` is now `ContentId` (was: `Guid`). Access the Guid via
`docRef.Value.Value`.

### 3. Handlers parse ContentId, not Guid

Four engine handlers updated to use `ContentId.TryParse`:
- `DefineEnforcementRuleHandler`
- `CreateAuditRecordHandler`
- `CreateKanbanCardHandler`
- `UpdateKanbanCardHandler`

Each throws a clear error if the command's content-id string is not a valid
non-empty Guid.

### 4. Schema mappers use `.Value.Value.ToString()`

- `EconomicSchemaModule.cs`: EnforcementRuleDefinedEvent, AuditRecordCreatedEvent
- `KanbanSchemaModule.cs`: KanbanCardCreatedEvent, KanbanCardDetailRevisedEvent

External schema shapes preserved — field remains `string`, value is the Guid
component of the canonical ContentId.

### 5. KanbanCard model corrected

Previously had single `DocumentRef Content` (from the earlier over-
externalisation). Now:
- `KanbanCardTitle Title` — typed structural VO, validated (non-empty, ≤256 chars).
- `DocumentRef Description` — externalised content reference.

Events (`KanbanCardCreatedEvent`, `KanbanCardDetailRevisedEvent`) carry both
fields. Aggregate `CreateCard` / `ReviseCardDetail` take `(title, description)`.
Errors: `CardTitleRequired`, `CardDescriptionRequired`, `InvalidCardDescriptionRef`.

## Promotable guard rules (green-locked in `ContentSystemEnforcementLockTests`)

### CS-DOCREF-WRAPS-CONTENTID-01 (S1)
Every local `DocumentRef` VO MUST wrap the canonical shared-kernel `ContentId`,
not a raw `Guid`. A `public Guid Value` inside a `DocumentRef.cs` file is a
violation.

**Test:** `Every_local_DocumentRef_wraps_ContentId_not_raw_Guid`.

### CS-DOCREF-MISUSE-01 (S1)
`DocumentRef` MUST NOT back any `*Name`, `*Label`, `*Code`, `*Identifier`, or
`*Id` property. Those are structural identity — use a typed VO instead.
`DocumentRef` is reserved for true evolving content.

**Test:** `DocumentRef_is_not_used_for_name_label_code_or_identifier_fields`.

### CS-KANBAN-TITLE-TYPED-01 (S1)
`KanbanCard.Title` MUST be a typed `KanbanCardTitle` VO — not `string` and not
`DocumentRef`. Titles are structural identity, not content.

### CS-KANBAN-TITLE-NOT-EXTERNALISED-01 (S2)
`KanbanCard.Title` MUST NOT be externalised to `DocumentRef` even if someone is
tempted by the prose-like surface. Title remains a typed VO.

### CS-DECOUPLE-KANBAN-01 (updated — S1)
`KanbanCard.Description` MUST be `DocumentRef`, not raw `string`. Externalised
content only.

**Test:** `KanbanCard_Title_is_typed_VO_and_Description_is_DocumentRef`.

## Backward compatibility (as applied)

- External wire schemas (`*EventSchema`, `*ReadModel`, `*Command`, request DTOs)
  — no shape changes. Fields remain same types.
- Semantic shift within existing fields:
  - `KanbanCardCreatedEventSchema.Title` / `KanbanCardUpdatedEventSchema.Title`
    now carry the typed title prose (previously carried the collapsed Guid).
  - `KanbanCardCreatedEventSchema.Description` / `KanbanCardUpdatedEventSchema.Description`
    now carry the `ContentId` Guid-as-string (previously empty after collapse).

The Title / Description shape on the external schema is now **better aligned**
with the pre-collapse shape — Title is title prose again, Description is a
content reference.

## Verification

- Full solution build: 0 warnings, 0 errors.
- Full unit test suite: **508 passed / 8 failed** — all failures are pre-existing,
  unrelated to this pass (infrastructure-path mismatches in
  `PolicyArtifactCoverageTests`).
- Arch / enforcement-lock tests: **23 passed / 0 failed** — 5 new tests all green
  (Kanban Title+Description split, ContentId wrap guard, DocumentRef misuse guard).

## Proposed promotion target

Add to `claude/guards/domain.guard.md` under a new "Content-System Identity"
subsection:

- CS-DOCREF-WRAPS-CONTENTID-01 — S1
- CS-DOCREF-MISUSE-01 — S1
- CS-KANBAN-TITLE-TYPED-01 — S1
- CS-KANBAN-TITLE-NOT-EXTERNALISED-01 — S2

The rules are already authoritative via the arch test suite; guard-text
promotion makes them discoverable by the `$1a` guard-loading sweep.
