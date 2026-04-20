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
