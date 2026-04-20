# progress (SCAFFOLD — pending implementation)

## Purpose

The `progress` leaf owns **viewer playback progress** — position, playhead events, and progress checkpoints for a viewer's consumption of a stream or archive.

## Owns (planned)

- Progress identity, session ref / replay ref, position, last-position-timestamp, status (Tracking / Paused / Terminated).
- Track / update-position / pause / resume / terminate transitions.

## Does not own

- The session itself — owned by `playback-consumption/session`.
- The replay lifecycle — owned by `playback-consumption/replay`.
- Viewer identity.
- Aggregate telemetry (bitrate, drop) — owned by `delivery-governance/observability`.

## Status

SCAFFOLD only in P2.6.CS.7.
