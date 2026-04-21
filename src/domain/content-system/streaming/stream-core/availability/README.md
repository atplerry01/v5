# playback

## Purpose

The `playback` leaf owns playback descriptors — the technical artifact that governs whether, how, and when a given source can be played back. It is distinct from access control (`control/access`) and from the stream itself.

## Aggregate root

- `PlaybackAggregate`

## Key value objects

- `PlaybackId`
- `PlaybackSourceRef`
- `PlaybackSourceKind`
- `PlaybackMode`
- `PlaybackWindow`
- `PlaybackStatus` (Created / Enabled / Disabled / Archived)

## Key events

- `PlaybackCreatedEvent`
- `PlaybackEnabledEvent`
- `PlaybackDisabledEvent`
- `PlaybackWindowUpdatedEvent`
- `PlaybackArchivedEvent`

## Invariants and lifecycle rules

- Created in `Created`.
- `Enable` rejects archived or already-enabled.
- `Disable` requires a non-empty reason; rejects archived or already-disabled.
- `UpdateWindow` rejects archived; no-op when the new window matches current.
- `Archive` is terminal.
- `SourceRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedPlayback`).

## Owns

- Playback descriptor identity, source reference and kind, mode, window, status.
- Create / enable / disable / update-window / archive transitions.

## References

- `PlaybackSourceRef` — opaque source being played back.
- `PlaybackSourceKind` — typed discriminator.

## Does not own

- Entitlement / commercial playback permission — those are not modelled in `content-system` at all.
- Technical access-grant truth — owned by `control/access`.
- The player, the player UI, or client SDKs — infrastructure.
- DRM licence issuance — policy / infrastructure.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanEnablePlaybackSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `PlaybackAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
