# stream-session

## Purpose

The `stream-session` leaf owns per-session lifecycle for a stream — one session per viewer attachment, with window enforcement and distinct terminal outcomes (closed, failed, expired).

## Aggregate root

- `StreamSessionAggregate`

## Key value objects

- `StreamSessionId`
- `StreamRef`
- `SessionWindow`
- `SessionStatus` (Opened / Active / Suspended / Closed / Failed / Expired)
- `SessionTerminationReason`

## Key events

- `StreamSessionOpenedEvent`
- `StreamSessionActivatedEvent`
- `StreamSessionSuspendedEvent`
- `StreamSessionResumedEvent`
- `StreamSessionClosedEvent`
- `StreamSessionFailedEvent`
- `StreamSessionExpiredEvent`

## Invariants and lifecycle rules

- `Open` rejects if the session window has already expired at open-time (`OpenedAfterExpiry`).
- `Activate`: valid only from `Opened`.
- `Suspend`: valid only from `Active`.
- `Resume`: valid only from `Suspended`.
- `Close`, `Fail`, `Expire`: each rejected if the session is already terminal.
- Terminal = `Closed` / `Failed` / `Expired`.
- `StreamRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedSession`).

## Owns

- Session identity, stream binding, window, status, termination reason, timestamps.
- Open / activate / suspend / resume / close / fail / expire transitions.

## References

- `StreamRef` — the stream this session attaches to.
- `SessionWindow` — the validity window for the session.

## Does not own

- Viewer identity — session does not carry a user reference; opaque to identity layer.
- Stream-level lifecycle — owned by `stream-core/stream` and `stream-core/live-stream`.
- Delivery artifacts — owned by `delivery-artifact/`.
- Access-grant truth — owned by `control/access`.

## Notes

- The three distinct terminal outcomes (`Closed`, `Failed`, `Expired`) are separate events so that downstream consumers can distinguish graceful closure from failure and timeout without inspecting reason strings.
