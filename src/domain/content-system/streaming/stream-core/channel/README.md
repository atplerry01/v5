# channel

## Purpose

The `channel` leaf owns a named channel bound to a stream — identity, stream binding, name, mode, and enabled/disabled/archived lifecycle.

## Aggregate root

- `ChannelAggregate`

## Key value objects

- `ChannelId`
- `StreamRef`
- `ChannelName`
- `ChannelMode`
- `ChannelStatus` (Created / Enabled / Disabled / Archived)

## Key events

- `ChannelCreatedEvent`
- `ChannelRenamedEvent`
- `ChannelEnabledEvent`
- `ChannelDisabledEvent`
- `ChannelArchivedEvent`

## Invariants and lifecycle rules

- Created in `Created`.
- `Rename` is rejected on archived; no-op when the new name matches current.
- `Enable` rejects already-enabled or archived.
- `Disable` requires a non-empty reason; rejects already-disabled or archived.
- Archived is terminal.
- `StreamRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedChannel`).

## Owns

- Channel identity, stream binding, name, mode, status.
- Rename / enable / disable / archive transitions.

## References

- `StreamRef` — the stream this channel is bound to.

## Does not own

- The stream — owned by `stream-core/stream`.
- Broadcast lifecycle — owned by `stream-core/live-stream`.
- Any consumer-facing channel catalogue / branding concerns.
- Channel-level access — handled via `control/access` at the stream level.
