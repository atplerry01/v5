# document

## Purpose

The `document` leaf owns the identity and lifecycle of a stored document — its title, type, classification, and status. It is the primary content artifact of the `document` context.

## Aggregate root

- `DocumentAggregate`

## Key value objects

- `DocumentId`
- `DocumentTitle`
- `DocumentType`
- `DocumentClassification`
- `DocumentStatus` (Draft / Active / Archived)

## Key events

- `DocumentCreatedEvent`
- `DocumentMetadataUpdatedEvent`
- `DocumentActivatedEvent`
- `DocumentArchivedEvent`
- `DocumentRestoredEvent`

## Invariants and lifecycle rules

- A newly created document starts in `Draft`.
- An archived document cannot be modified or re-archived; it can be restored to `Active`.
- `Activate` is valid only from non-archived states; double-activate is rejected.
- `UpdateMetadata` is a no-op when the new title, type, and classification match current values (no event emitted).
- `Restore` is valid only from `Archived`.

## Owns

- Document identity, title, type, classification, status.
- Create / metadata-update / activate / archive / restore transitions.

## References

- No local `*Ref` value objects — the document is a root artifact; other domains reference it via their own `DocumentRef` value objects.

## Does not own

- The stored file bytes — owned by `document/core-object/file`.
- Version lineage — owned by `document/lifecycle/version` (moves to `lifecycle-change/version` in CS.2).
- Metadata entries — owned by `document/descriptor/metadata`.
- Retention — owned by `document/lifecycle/retention` (moves to `governance/retention` in CS.2).
- Workflow and approval meaning.
- Classification DECISIONS, approvals, and challenge lifecycle — owned by `document/governance/classification` (scaffolded in CS.3 per §CD-16). The `DocumentClassification` VO on this aggregate represents the currently-ASSIGNED classification (cached state), mutated only in reaction to `ClassificationApplied`/`ClassificationRevised`/`ClassificationRevoked` events from that domain.

## §CD-16 disambiguation note

Two concerns, two owners:

- `document/core-object/document.DocumentClassification` (VO, here) — the currently-assigned classification value as CACHED STATE on the document aggregate. Read-optimised attribute. Mutated only in response to classification-decision events from the governance domain.
- `document/governance/classification` (aggregate, scaffolded CS.3) — OWNS the decision lifecycle: propose / apply / revise / revoke / challenge.

`DocumentAggregate` MUST NOT mutate its `DocumentClassification` VO via local commands. Violation would be a boundary bleed.

## Notes

- The aggregate is event-sourced; state is rebuilt from its event log via `Apply`.
