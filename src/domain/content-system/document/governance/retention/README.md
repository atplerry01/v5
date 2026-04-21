# retention

## Purpose

The `retention` leaf owns the attachment of a retention policy to a target and the retention lifecycle that follows — applied, held, released, expired, marked-eligible-for-destruction, archived.

## Aggregate root

- `RetentionAggregate`

## Key value objects

- `RetentionId`
- `RetentionTargetRef`
- `RetentionTargetKind`
- `RetentionWindow`
- `RetentionReason`
- `RetentionStatus` (Applied / Held / Released / Expired / EligibleForDestruction / Archived)

## Key events

- `RetentionAppliedEvent`
- `RetentionHoldPlacedEvent`
- `RetentionReleasedEvent`
- `RetentionExpiredEvent`
- `RetentionMarkedEligibleForDestructionEvent`
- `RetentionArchivedEvent`

## Invariants and lifecycle rules

- `Apply` creates the retention in status `Applied` with a `RetentionWindow` and `RetentionReason`.
- `PlaceHold` is valid only from `Applied`; rejects already-held, archived, or terminal states.
- `Release` is valid only from `Applied` or `Held`; rejects already-released or archived.
- `Expire` rejects already-expired / eligible-for-destruction / archived states.
- `MarkEligibleForDestruction` is valid only from `Expired`.
- `Archive` is terminal; double-archive rejected.
- `TargetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedRetention`).

## Owns

- Retention identity, target ref + kind, window, reason, status, timestamps.
- Apply / place-hold / release / expire / mark-eligible / archive transitions.

## References

- `RetentionTargetRef` — opaque reference to the target (document, record, file, etc.).
- `RetentionTargetKind` — typed discriminator of what the target is.

## Does not own

- The target itself.
- The actual destruction / purge — the aggregate only marks eligibility; destruction is orchestrated downstream.
- Legal-hold semantics beyond the aggregate's own `Held` state.
- Retention policy evaluation (duration, regulatory class) — that is policy-engine logic; this aggregate only records the applied window.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanReleaseRetentionSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `RetentionAggregate.Apply(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
