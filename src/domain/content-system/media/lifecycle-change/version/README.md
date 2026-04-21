# version

## Purpose

The `version` leaf owns media version lineage — each version is a distinct aggregate tracking its version number, its required backing `MediaFileRef`, its previous/successor links, and its draft/active/superseded/withdrawn status.

## Aggregate root

- `MediaVersionAggregate`

## Key value objects

- `MediaVersionId`
- `MediaAssetRef`
- `MediaVersionNumber`
- `MediaFileRef` (mandatory)
- `MediaVersionStatus` (Draft / Active / Superseded / Withdrawn)

## Key events

- `MediaVersionCreatedEvent`
- `MediaVersionActivatedEvent`
- `MediaVersionSupersededEvent`
- `MediaVersionWithdrawnEvent`

## Invariants and lifecycle rules

- A version is created in `Draft` with an optional `PreviousVersionId` and a **required** `MediaFileRef`.
- `Activate` is valid only from `Draft`; rejects already-active or non-draft.
- `Supersede` is valid only from `Active`; rejects non-active, already-superseded, or supersede-with-self.
- `Withdraw` requires a non-empty reason; double-withdraw rejected.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedMediaVersion`).

## Owns

- Version identity, asset back-pointer, version number, file ref, previous/successor linkage, status.
- Create / activate / supersede / withdraw transitions.

## References

- `MediaAssetRef` — the owning media asset.
- `MediaFileRef` — mandatory backing file for this version.

## Does not own

- The asset — owned by `content-artifact/asset`.
- The backing file — owned by `content-artifact/media-file`.
- Diff / comparison between versions.

## Notes

- Unlike `document/lifecycle/version` which uses a general-purpose `ArtifactRef`, `media/lifecycle/version` requires a concrete `MediaFileRef`: a media version is always bound to a specific backing media file.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanActivateMediaVersionSpecification`, `CanSupersedeMediaVersionSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `MediaVersionAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
