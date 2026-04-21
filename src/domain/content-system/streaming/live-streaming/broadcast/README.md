# live-stream

## Purpose

The `live-stream` leaf owns broadcast lifecycle for live streaming — scheduling, start, pause, resume, end, and cancel. It is a dedicated broadcast aggregate distinct from the generic `stream`.

## Aggregate root

- `LiveStreamAggregate`

## Key value objects

- `LiveStreamId`
- `StreamRef`
- `LiveStreamStatus` (Created / Scheduled / Live / Paused / Ended / Cancelled)
- `LiveBroadcastWindow`

## Key events

- `LiveStreamCreatedEvent`
- `LiveStreamScheduledEvent`
- `LiveStreamStartedEvent`
- `LiveStreamPausedEvent`
- `LiveStreamResumedEvent`
- `LiveStreamEndedEvent`
- `LiveStreamCancelledEvent`

## Invariants and lifecycle rules

- Created in `Created`.
- `Schedule`: rejected if terminal (`Ended`, `Cancelled`) or already `Live` / `Paused`.
- `Start`: rejected if terminal, already `Live`, or `Paused`.
- `Pause`: valid only from `Live`.
- `Resume`: valid only from `Paused`.
- `End`: valid only from `Live` or `Paused`.
- `Cancel`: requires non-empty reason; rejected if terminal.
- `StreamRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedLiveStream`).

## Owns

- Live-stream identity, schedule window, broadcast status, started/ended timestamps.
- Schedule / start / pause / resume / end / cancel transitions.

## References

- `StreamRef` — the root stream this broadcast belongs to.
- `LiveBroadcastWindow` — scheduled broadcast window.

## Does not own

- The stream itself — owned by `stream-core/stream`.
- The channel — owned by `stream-core/channel`.
- Session behaviour per viewer — owned by `stream-core/stream-session`.
- Delivery-side manifest/segment/playback — owned by `delivery-artifact/`.
- Recording — owned by `persistence-and-observability/recording`.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanStartBroadcastSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `BroadcastAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
