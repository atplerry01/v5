# stream

## Purpose

The `stream` leaf owns the root streaming aggregate — the identity, mode, type, and coarse lifecycle of a stream. Every other domain in `streaming` ultimately refers back to a stream.

## Aggregate root

- `StreamAggregate`

## Key value objects

- `StreamId`
- `StreamMode`
- `StreamType`
- `StreamStatus` (Created / Active / Paused / Ended / Archived)

## Key events

- `StreamCreatedEvent`
- `StreamActivatedEvent`
- `StreamPausedEvent`
- `StreamResumedEvent`
- `StreamEndedEvent`
- `StreamArchivedEvent`

## Invariants and lifecycle rules

- Created in `Created`.
- `Activate`: valid only from `Created` / `Paused` transitions; rejected on `Active`, `Ended`, `Archived`.
- `Pause`: valid only from `Active`.
- `Resume`: valid only from `Paused`.
- `End`: valid only from `Active` or `Paused`.
- `Archive`: valid only from `Ended`; rejected otherwise.
- Archived is terminal.

## Owns

- Stream identity, mode, type, status, started/ended timestamps.
- Activate / pause / resume / end / archive transitions.

## References

- No local `*Ref` — other streaming domains reference this aggregate via their own `StreamRef` value objects.

## Does not own

- Broadcast scheduling — owned by `stream-core/live-stream`.
- Channel binding / naming — owned by `stream-core/channel`.
- Session management — owned by `stream-core/stream-session`.
- Delivery artifacts — owned by `delivery-artifact/`.
- Access grants — owned by `control/access`.
- Recording / metrics — owned by `persistence-and-observability/`.
