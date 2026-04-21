# version

## Purpose

The `version` leaf owns document version lineage — each version is a distinct aggregate tracking its version number, its backing artifact, its previous/successor links, and its draft/active/superseded/withdrawn status.

## Aggregate root

- `DocumentVersionAggregate`

## Key value objects

- `DocumentVersionId`
- `DocumentRef`
- `VersionNumber`
- `ArtifactRef`
- `VersionStatus` (Draft / Active / Superseded / Withdrawn)

## Key events

- `DocumentVersionCreatedEvent`
- `DocumentVersionActivatedEvent`
- `DocumentVersionSupersededEvent`
- `DocumentVersionWithdrawnEvent`

## Invariants and lifecycle rules

- A version is created in `Draft` with an optional `PreviousVersionId`.
- `Activate` is valid only from `Draft`; rejects already-active or non-draft.
- `Supersede` is valid only from `Active`; rejects non-active, already-superseded, or supersede-with-self.
- `Withdraw` requires a non-empty reason; double-withdraw rejected.
- `DocumentRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedVersion`).

## Owns

- Version identity, document back-pointer, version number, artifact ref, previous/successor linkage, status.
- Create / activate / supersede / withdraw transitions.

## References

- `DocumentRef` — back-pointer to the owning document.
- `ArtifactRef` — reference to the concrete backing artifact for this version. The aggregate uses a general-purpose `ArtifactRef` so versions can point at document files, bundles, or other document-centric artifacts.

## Does not own

- The document — owned by `document/core-object/document`.
- The backing artifact (file/bundle) — owned by its respective leaf.
- Diff or comparison computation between versions — outside the domain layer.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanActivateSpecification`, `CanSupersedeSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `DocumentVersionAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
