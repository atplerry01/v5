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
