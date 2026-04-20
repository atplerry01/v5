# streaming / live-streaming

## Purpose

Groups **live broadcast** concerns — broadcast lifecycle, live ingest, and live archive. Distinct from stream identity (`stream-core/`), which covers the long-lived stream descriptor.

## Leaf domains

- `broadcast/` — live broadcast lifecycle (Created / Scheduled / Live / Paused / Ended / Cancelled). Moved from `stream-core/live-stream` per §CD-08 pattern. Aggregate class `LiveStreamAggregate` retains its name pending CS.13 Band-F rename.
- `archive/` — (SCAFFOLD produced by CS.6) authoritative persisted broadcast output — "this broadcast was recorded and stored." Split from legacy `persistence-and-observability/recording`.
- `ingest-session/` — (SCAFFOLD pending CS.7) live ingest session.

## Boundary notes

- Broadcast truth ≠ stream truth. A stream can exist without a live broadcast (pre-scheduled, VOD-style). A broadcast is the live-ramp behaviour attached to a stream.
- Archive (CS.6) vs Replay: `archive/` is broadcast-side persistence; `replay/` (in `playback-consumption/`) is viewer-side re-watch lifecycle.
