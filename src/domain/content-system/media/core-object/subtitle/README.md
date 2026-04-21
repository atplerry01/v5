# subtitle

## Purpose

The `subtitle` leaf owns subtitle companion artifacts for a media asset — format, language, optional timing window, and the produced subtitle output reference. Draft → Active → Finalized → Archived.

## Aggregate root

- `SubtitleAggregate`

## Key value objects

- `SubtitleId`
- `MediaAssetRef`
- `MediaFileRef` (optional — source file)
- `SubtitleFormat`
- `SubtitleLanguage`
- `SubtitleWindow` (optional — timing window)
- `SubtitleOutputRef`
- `SubtitleStatus` (Draft / Active / Finalized / Archived)

## Key events

- `SubtitleCreatedEvent`
- `SubtitleUpdatedEvent`
- `SubtitleFinalizedEvent`
- `SubtitleArchivedEvent`

## Invariants and lifecycle rules

- Created in `Draft` with optional source file and optional timing window.
- `Update` sets the `SubtitleOutputRef` and moves status to `Active`; rejected on finalized or archived.
- `Finalize` rejects already-finalized or archived.
- Archived is terminal.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedSubtitle`).

## Owns

- Subtitle identity, language, format, window, output ref, status.
- Create / update / finalize / archive transitions.

## References

- `MediaAssetRef` — the asset this subtitle accompanies.
- `MediaFileRef` (optional) — the source media file this subtitle was derived from.
- `SubtitleOutputRef` — the produced subtitle artifact.

## Does not own

- The media asset — owned by `content-artifact/asset`.
- The ASR / translation engine — infrastructure.
- The subtitle rendering / display pipeline — player-side concern.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanModifySubtitleSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `SubtitleAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
