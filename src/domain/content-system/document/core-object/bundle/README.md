# bundle

## Purpose

The `bundle` leaf owns named groupings of document-centric members. A bundle is a finite, finalisable set of references with an open/finalized/archived lifecycle.

## Aggregate root

- `DocumentBundleAggregate`

## Key value objects

- `DocumentBundleId`
- `BundleName`
- `BundleStatus` (Open / Finalized / Archived)
- `BundleMemberRef`

## Key events

- `DocumentBundleCreatedEvent`
- `DocumentBundleRenamedEvent`
- `DocumentBundleMemberAddedEvent`
- `DocumentBundleMemberRemovedEvent`
- `DocumentBundleFinalizedEvent`
- `DocumentBundleArchivedEvent`

## Invariants and lifecycle rules

- A finalized or archived bundle is immutable — rename and add/remove-member are rejected.
- A bundle cannot be finalized while empty (`EmptyBundle`).
- Duplicate member adds are rejected (`DuplicateMember`); removing an unknown member is rejected (`UnknownMember`).
- Rename is a no-op when the new name matches current.

## Owns

- Bundle identity, name, member set, status, finalized-at timestamp.
- Rename / add-member / remove-member / finalize / archive transitions.

## References

- `BundleMemberRef` — opaque member reference within the bundle; the bundle does not resolve member semantics.

## Does not own

- The documents that members point at — owned by `document/core-object/document`.
- Any ordering beyond set membership — bundles are unordered.
- Workflow meaning attached to the bundle (submission, packet-delivery, deposition) — those are workflow concerns.

## Notes

- Membership is represented as a `HashSet<BundleMemberRef>`; the event log is the source of truth and hydration replays adds/removes.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanModifyBundleSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `DocumentBundleAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
